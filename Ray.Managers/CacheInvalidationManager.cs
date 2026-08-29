using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers.Helpers;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Utils.Cache;
using Ray.Utils.Logging;
using Ray.Utils.Solr;
using Ray.Utils.Text;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public class NewsCacheSnapshot
    {
        public NewsCacheSnapshot()
        {
            SectionSlugs = new List<string>();
            SectionPaths = new List<string>();
        }

        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsAlert { get; set; }
        public IList<string> SectionSlugs { get; set; }
        public IList<string> SectionPaths { get; set; }
    }

    public interface ICacheInvalidationManager
    {
        Task<NewsCacheSnapshot> CaptureNewsSnapshot(int newsId);
        void InvalidateNews(int newsId, NewsCacheSnapshot previous = null, string reason = null);
        void InvalidateNews(IEnumerable<int> newsIds, string reason = null);
        void InvalidateLayout(int nodeId, string reason = null);
        void InvalidatePaths(IEnumerable<string> paths, string reason = null);
        Task<CacheInvalidationResult> InvalidateNewsNow(int newsId, NewsCacheSnapshot previous = null, string reason = null);
        Task<CacheInvalidationResult> InvalidateLayoutNow(int nodeId, string reason = null);
        Task<CacheInvalidationResult> InvalidatePathsNow(IEnumerable<string> paths, string reason = null);
    }

    public class CacheInvalidationManager : ICacheInvalidationManager
    {
        private static readonly object PortadaIndexLock = new object();
        private static Dictionary<int, HashSet<int>> _portadaIndex;
        private static DateTime _portadaIndexExpiresAtUtc = DateTime.MinValue;

        private readonly IAssetRepository _assets;
        private readonly INodeRepository _nodes;
        private readonly ILayoutInstanceRepository _layoutInstances;
        private readonly AppSettings _appSettings;

        public CacheInvalidationManager(
            IAssetRepository assets,
            INodeRepository nodes,
            ILayoutInstanceRepository layoutInstances,
            AppSettings appSettings)
        {
            _assets = assets;
            _nodes = nodes;
            _layoutInstances = layoutInstances;
            _appSettings = appSettings;
        }

        public async Task<NewsCacheSnapshot> CaptureNewsSnapshot(int newsId)
        {
            try
            {
                return await ReadSnapshotFromSolr(newsId) ?? await ReadSnapshotFromDatabase(newsId);
            }
            catch (Exception ex)
            {
                CMSLogger.Error($"[cache-invalidation] snapshot failed for news {newsId}: {ex.Message}");
                return null;
            }
        }

        public void InvalidateNews(int newsId, NewsCacheSnapshot previous = null, string reason = null)
        {
            RunDetached(() => InvalidateNewsNow(newsId, previous, reason ?? $"news:{newsId}"));
        }

        public void InvalidateNews(IEnumerable<int> newsIds, string reason = null)
        {
            var ids = newsIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return;

            RunDetached(async () =>
            {
                var paths = new List<string>();
                foreach (var id in ids)
                    paths.AddRange(await BuildNewsPaths(id, null));

                return await FrontendCacheClient.Invalidate(paths, _appSettings, reason ?? $"news-batch:{ids.Count}", CacheInvalidationKind.News);
            });
        }

        public void InvalidateLayout(int nodeId, string reason = null)
        {
            RunDetached(() => InvalidateLayoutNow(nodeId, reason ?? $"layout:{nodeId}"));
        }

        public void InvalidatePaths(IEnumerable<string> paths, string reason = null)
        {
            RunDetached(() => InvalidatePathsNow(paths, reason));
        }

        public async Task<CacheInvalidationResult> InvalidateNewsNow(int newsId, NewsCacheSnapshot previous = null, string reason = null)
        {
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            var paths = await BuildNewsPaths(newsId, previous);
            return await FrontendCacheClient.Invalidate(paths, _appSettings, reason ?? $"news:{newsId}", CacheInvalidationKind.News);
        }

        public async Task<CacheInvalidationResult> InvalidateLayoutNow(int nodeId, string reason = null)
        {
            InvalidatePortadaIndex();

            var paths = new List<string>();
            var nodePath = await GetNodePath(nodeId);
            if (nodePath != null)
                paths.Add(nodePath);

            paths.Add($"/api/layoutinstance/{nodeId}");
            paths.Add("/api/node/getMain");

            return await FrontendCacheClient.Invalidate(paths, _appSettings, reason ?? $"layout:{nodeId}", CacheInvalidationKind.Layout);
        }

        public async Task<CacheInvalidationResult> InvalidatePathsNow(IEnumerable<string> paths, string reason = null)
        {
            return await FrontendCacheClient.Invalidate(paths, _appSettings, reason ?? "manual paths");
        }

        private async Task<List<string>> BuildNewsPaths(int newsId, NewsCacheSnapshot previous)
        {
            var paths = new List<string> { $"/api/news/{newsId}" };
            var current = await ReadSnapshotFromSolr(newsId) ?? await ReadSnapshotFromDatabase(newsId);

            AddSnapshotPaths(paths, current, newsId);
            AddSnapshotPaths(paths, previous, newsId);

            var isAlert = (current != null && current.IsAlert) || (previous != null && previous.IsAlert);
            if (isAlert)
            {
                paths.Add("/api/news/getAlert");
                paths.Add("/");
            }

            foreach (var nodeId in await GetNodesWithNewsInPortada(newsId))
            {
                var nodePath = await GetNodePath(nodeId);
                if (nodePath != null)
                    paths.Add(nodePath);

                paths.Add($"/api/layoutinstance/{nodeId}");
            }

            return paths;
        }

        private static void AddSnapshotPaths(List<string> paths, NewsCacheSnapshot snapshot, int newsId)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.Url))
                return;

            var slugs = snapshot.SectionSlugs?.Count > 0 ? snapshot.SectionSlugs : new List<string> { "seccion" };
            foreach (var slug in slugs.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                paths.Add($"/{slug}/{snapshot.Url}_{newsId}");
                paths.Add($"/amp/{slug}/{snapshot.Url}_{newsId}");
            }

            if (snapshot.SectionPaths?.Count > 0)
            {
                foreach (var sectionPath in snapshot.SectionPaths.Where(x => !string.IsNullOrWhiteSpace(x)))
                    paths.Add("/" + sectionPath.Trim('/'));
            }
            else
            {
                foreach (var slug in slugs.Where(x => !string.IsNullOrWhiteSpace(x)))
                    paths.Add($"/{slug}");
            }
        }

        private async Task<string> GetNodePath(int nodeId)
        {
            try
            {
                var node = await _nodes.GetById(nodeId);
                if (node == null || node.IsDeleted)
                    return null;

                if (node.ParentNodeId == null)
                    return "/";

                return string.IsNullOrWhiteSpace(node.Description) ? null : "/" + node.Description.Trim('/');
            }
            catch (Exception ex)
            {
                CMSLogger.Error($"[cache-invalidation] node path failed for {nodeId}: {ex.Message}");
                return null;
            }
        }

        private async Task<NewsCacheSnapshot> ReadSnapshotFromSolr(int newsId)
        {
            var raw = await SolrHelper.ExecuteQuery(SolrCore.NEWS,
                $"q=Id:{newsId}&rows=1&fl=Id,Url,Nodes_en,Nodes_slug,IsAlert,Status",
                _appSettings.Solr);

            var docs = (raw as JObject)?["response"]?["docs"] as JArray;
            var first = docs?.FirstOrDefault() as JObject;
            if (first == null)
                return null;

            var url = first.Value<string>("Url");
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var snapshot = new NewsCacheSnapshot
            {
                Id = newsId,
                Url = url,
                IsAlert = first.Value<bool?>("IsAlert") ?? false
            };

            var nodesEn = (first["Nodes_en"] as JArray)?.Select(x => x.Value<string>()).ToList();
            snapshot.SectionSlugs.Add(FrontSlug.SectionSlug(nodesEn));

            var nodesSlug = (first["Nodes_slug"] as JArray)?.Select(x => x.Value<string>()).ToList();
            if (nodesSlug != null)
            {
                foreach (var nodeSlug in nodesSlug.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    var clean = nodeSlug.Trim().Trim('/');
                    if (clean.Length > 0 && !snapshot.SectionPaths.Contains(clean))
                        snapshot.SectionPaths.Add(clean);
                }
            }

            return snapshot;
        }

        private async Task<NewsCacheSnapshot> ReadSnapshotFromDatabase(int newsId)
        {
            var assets = await _assets.Get(x => x.Id == newsId);
            var asset = await assets.OfType<News>()
                .Include(x => x.AssetNodes)
                .ThenInclude(x => x.Node)
                .ThenInclude(x => x.Content)
                .ThenInclude(x => x.Language)
                .FirstOrDefaultAsync();

            if (asset == null || string.IsNullOrWhiteSpace(asset.Url))
                return null;

            var snapshot = new NewsCacheSnapshot
            {
                Id = newsId,
                Url = asset.Url,
                IsAlert = asset.IsAlert
            };

            foreach (var assetNode in asset.AssetNodes)
            {
                var node = assetNode.Node;
                var title = GetNodeTitle(node);
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var slug = FrontSlug.Slugify(title);
                    if (!string.IsNullOrWhiteSpace(slug) && !snapshot.SectionSlugs.Contains(slug))
                        snapshot.SectionSlugs.Add(slug);
                }

                if (!string.IsNullOrWhiteSpace(node?.Description))
                {
                    var sectionPath = node.Description.Trim().Trim('/');
                    if (sectionPath.Length > 0 && !snapshot.SectionPaths.Contains(sectionPath))
                        snapshot.SectionPaths.Add(sectionPath);
                }
            }

            if (snapshot.SectionSlugs.Count == 0)
                snapshot.SectionSlugs.Add("seccion");

            return snapshot;
        }

        private async Task<IList<int>> GetNodesWithNewsInPortada(int newsId)
        {
            try
            {
                var index = await GetPortadaIndex();
                return index.Where(pair => pair.Value.Contains(newsId)).Select(pair => pair.Key).ToList();
            }
            catch (Exception ex)
            {
                CMSLogger.Error($"[cache-invalidation] portada lookup failed for news {newsId}: {ex.Message}");
                return new List<int>();
            }
        }

        private async Task<Dictionary<int, HashSet<int>>> GetPortadaIndex()
        {
            lock (PortadaIndexLock)
            {
                if (_portadaIndex != null && DateTime.UtcNow < _portadaIndexExpiresAtUtc)
                    return _portadaIndex;
            }

            var built = await BuildPortadaIndex();

            lock (PortadaIndexLock)
            {
                _portadaIndex = built;
                _portadaIndexExpiresAtUtc = DateTime.UtcNow.AddMinutes(10);
                return _portadaIndex;
            }
        }

        private async Task<Dictionary<int, HashSet<int>>> BuildPortadaIndex()
        {
            var index = new Dictionary<int, HashSet<int>>();
            var query = await _layoutInstances.Get(x => x.IsEnabled && !x.IsDeleted);
            var instances = await query.Select(x => new { x.NodeId, x.Structure }).ToListAsync();

            foreach (var instance in instances)
            {
                if (string.IsNullOrWhiteSpace(instance.Structure))
                    continue;

                try
                {
                    var structure = JsonConvert.DeserializeObject<LayoutStructureDto>(instance.Structure);
                    var ids = AssetHelper.GetNewsIds(structure);

                    if (!index.TryGetValue(instance.NodeId, out var bucket))
                    {
                        bucket = new HashSet<int>();
                        index[instance.NodeId] = bucket;
                    }

                    foreach (var id in ids)
                        bucket.Add(id);
                }
                catch (Exception ex)
                {
                    CMSLogger.Warn($"[cache-invalidation] invalid layout structure for node {instance.NodeId}: {ex.Message}");
                }
            }

            return index;
        }

        private static string GetNodeTitle(Node node)
        {
            var contents = node?.Content?.ToList();
            if (contents == null || contents.Count == 0)
                return null;

            var english = contents.FirstOrDefault(c =>
                c.Language != null &&
                !string.IsNullOrEmpty(c.Language.CultureName) &&
                c.Language.CultureName.StartsWith("en", StringComparison.OrdinalIgnoreCase));

            return (english ?? contents.First()).Title;
        }

        private static void InvalidatePortadaIndex()
        {
            lock (PortadaIndexLock)
            {
                _portadaIndex = null;
                _portadaIndexExpiresAtUtc = DateTime.MinValue;
            }
        }

        private static void RunDetached(Func<Task<CacheInvalidationResult>> work)
        {
            Task.Run(async () =>
            {
                try
                {
                    await work();
                }
                catch (Exception ex)
                {
                    CMSLogger.Error($"[cache-invalidation] background work failed: {ex}");
                }
            });
        }
    }
}
