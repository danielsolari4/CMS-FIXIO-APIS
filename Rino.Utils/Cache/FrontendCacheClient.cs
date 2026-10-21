using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rino.Dtos.Configuration;
using Rino.Utils.Logging;

namespace Rino.Utils.Cache
{
    public static class FrontendCacheClient
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        /// <param name="hard">
        /// true cuando el contenido dejó de ser público (borrado, despublicado o privado): el front
        /// borra su respaldo en memoria en vez de vencerlo, para que no pueda volver a servirse.
        /// </param>
        public static async Task<CacheInvalidationResult> Invalidate(
            IEnumerable<string> paths,
            AppSettings appSettings,
            string reason = null,
            CacheInvalidationKind kind = CacheInvalidationKind.News,
            bool hard = false)
        {
            var result = new CacheInvalidationResult { Reason = reason };
            var normalized = NormalizePaths(paths);
            result.Paths = normalized;

            if (normalized.Count == 0)
                return result;

            var nodes = GetFrontendInstances(appSettings);
            if (nodes.Count == 0)
            {
                CMSLogger.Error("[cache-invalidation] no frontend url configured.");
                return result;
            }

            var token = GetToken(appSettings);
            var mode = string.IsNullOrWhiteSpace(token) ? CacheInvalidationMode.LegacyNext : CacheInvalidationMode.Batch;
            result.Mode = mode.ToString();

            // El CDN se purga recién cuando todos los nodos ya tienen la versión nueva. Si se purgara
            // antes, una visita que el balanceador manda a un nodo todavía viejo volvería a llenar
            // el CDN con la página vieja hasta que venza su TTL.
            var cdn = ResolveCdnPlan(appSettings, nodes);

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < nodes.Count; i++)
            {
                var purgeCdn = cdn.Enabled && cdn.WithLastNode && i == nodes.Count - 1;
                result.Instances.Add(await InvalidateOne(nodes[i], normalized, purgeCdn, cdnOnly: false, mode, token, reason, kind, appSettings, hard));
            }

            if (cdn.Enabled && !cdn.WithLastNode)
                result.Instances.Add(await InvalidateOne(cdn.Url, normalized, purgeCdn: true, cdnOnly: true, mode, token, reason, kind, appSettings, hard));

            stopwatch.Stop();
            result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        private static async Task<CacheInvalidationInstanceResult> InvalidateOne(
            string baseUrl,
            IList<string> paths,
            bool purgeCdn,
            bool cdnOnly,
            CacheInvalidationMode mode,
            string token,
            string reason,
            CacheInvalidationKind kind,
            AppSettings appSettings,
            bool hard)
        {
            if (mode != CacheInvalidationMode.Batch)
                return await SendLegacy(baseUrl, paths, reason, kind, appSettings, hard, purgeCdn);

            var instance = await PostBatch(baseUrl, paths, purgeCdn, token, reason, appSettings, hard, cdnOnly);

            // Redundancia por diseño: si la forma moderna falla (front caído, token o CDN del
            // frente), se cae al legacy /api/ops/rebuild para no dejar el caché sin purgar.
            if (!instance.Success)
            {
                var legacy = await SendLegacy(baseUrl, paths, reason, kind, appSettings, hard, purgeCdn);
                legacy.UsedFallbackMode = legacy.Success ? "batch→legacy" : "batch→legacy(failed)";
                instance = legacy;
            }

            return instance;
        }

        private sealed class CdnPlan
        {
            public bool Enabled { get; set; }
            public bool WithLastNode { get; set; }
            public string Url { get; set; }
        }

        /// <summary>
        /// Decide si se purga el CDN y desde dónde. Un cliente con un solo servidor que hace todo
        /// (nodo y CDN) termina en una única llamada, igual que antes.
        /// </summary>
        private static CdnPlan ResolveCdnPlan(AppSettings appSettings, IList<string> nodes)
        {
            var settings = appSettings?.CacheInvalidation;
            bool enabled;
            if (bool.TryParse(GetSetting("FrontEnd__CacheInvalidation__PurgeCdn"), out var envEnabled))
                enabled = envEnabled;
            else if (settings?.Cdn?.Enabled.HasValue == true)
                enabled = settings.Cdn.Enabled.Value;
            else
                enabled = settings?.PurgeCdn ?? true;

            var url = (GetSetting("FrontEnd__CacheInvalidation__CdnUrl") ?? settings?.Cdn?.Url)?.Trim();
            var lastNode = nodes[nodes.Count - 1];
            var withLastNode = string.IsNullOrWhiteSpace(url) || string.Equals(url.TrimEnd('/'), lastNode.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);

            return new CdnPlan { Enabled = enabled, WithLastNode = withLastNode, Url = withLastNode ? lastNode : url };
        }

        /// <summary>
        /// Limpia el caché del middleware de redirects en cada nodo del front. NO tiene variante
        /// legacy (el /api/ops/rebuild viejo no purgeaba redirects), así que requiere token.
        /// Path del endpoint configurable vía FrontEnd__CacheInvalidation__RedirectsEndpoint.
        /// </summary>
        public static async Task<CacheInvalidationResult> InvalidateRedirects(AppSettings appSettings, string reason = null, IEnumerable<string> paths = null)
        {
            var result = new CacheInvalidationResult { Reason = reason };
            var normalized = NormalizePaths(paths);
            result.Paths = normalized;

            var instances = GetFrontendInstances(appSettings);
            if (instances.Count == 0)
            {
                CMSLogger.Error("[cache-invalidation] no frontend url configured for redirects.");
                return result;
            }

            var token = GetToken(appSettings);
            if (string.IsNullOrWhiteSpace(token))
            {
                result.Mode = "Redirects(no-token)";
                CMSLogger.Error("[cache-invalidation] redirects invalidation requires FrontEnd__CacheInvalidation__Token; there is no legacy fallback for /api/redirects/cache.");
                return result;
            }

            result.Mode = "Redirects";
            var cdn = ResolveCdnPlan(appSettings, instances);
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < instances.Count; i++)
            {
                // Igual que en Invalidate: el CDN se purga una sola vez, en la última llamada.
                var purgeCdn = normalized.Count > 0 && cdn.Enabled && i == instances.Count - 1;
                result.Instances.Add(await PostRedirects(instances[i], token, appSettings, normalized, purgeCdn));
            }

            stopwatch.Stop();
            result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        private static async Task<CacheInvalidationInstanceResult> PostBatch(
            string baseUrl,
            IList<string> paths,
            bool purgeCdn,
            string token,
            string reason,
            AppSettings appSettings,
            bool hard,
            bool cdnOnly)
        {
            var endpointPath = GetSetting("FrontEnd__CacheInvalidation__Endpoint");
            if (string.IsNullOrWhiteSpace(endpointPath))
                endpointPath = appSettings?.CacheInvalidation?.Endpoint;
            if (string.IsNullOrWhiteSpace(endpointPath))
                endpointPath = "/api/ops/cache-refresh";

            var payload = JsonConvert.SerializeObject(new
            {
                paths,
                purgeCdn,
                reason = reason ?? string.Empty,
                hard,
                cdnOnly
            });

            var url = baseUrl.TrimEnd('/') + endpointPath;
            var result = await SendWithRetries(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", "Bearer " + token);
                return request;
            }, baseUrl, appSettings);

            result.PurgedCdn = purgeCdn;
            return result;
        }

        private static async Task<CacheInvalidationInstanceResult> SendLegacy(
            string baseUrl,
            IList<string> paths,
            string reason,
            CacheInvalidationKind kind,
            AppSettings appSettings,
            bool hard,
            bool purgeCdn)
        {
            var instanceResult = new CacheInvalidationInstanceResult { Url = baseUrl, PurgedCdn = purgeCdn };
            var root = baseUrl.TrimEnd('/');
            var secret = GetSecret(appSettings);
            if (string.IsNullOrWhiteSpace(secret))
                secret = "rinocms!";

            var errors = new List<string>();
            foreach (var path in paths)
            {
                var url = $"{root}/api/ops/rebuild?secret={Uri.EscapeDataString(secret)}&nocache={DateTime.UtcNow.Ticks}&path={Uri.EscapeDataString(path)}&purgeCdn={(purgeCdn ? "true" : "false")}" + (hard ? "&hard=true" : string.Empty);
                var single = await SendWithRetries(() => new HttpRequestMessage(HttpMethod.Get, url), baseUrl, appSettings);
                instanceResult.Attempts += single.Attempts;

                if (!single.Success)
                {
                    instanceResult.StatusCode = single.StatusCode;
                    errors.Add($"{path}: {single.StatusCode} {single.Error}");
                }
            }

            instanceResult.Success = errors.Count == 0;
            if (errors.Count > 0)
                instanceResult.Error = string.Join(" | ", errors);

            return instanceResult;
        }

        private static async Task<CacheInvalidationInstanceResult> PostRedirects(string baseUrl, string token, AppSettings appSettings, IList<string> paths, bool purgeCdn)
        {
            var endpointPath = GetSetting("FrontEnd__CacheInvalidation__RedirectsEndpoint");
            if (string.IsNullOrWhiteSpace(endpointPath))
                endpointPath = "/api/redirects/cache";

            var payload = JsonConvert.SerializeObject(new
            {
                paths,
                purgeCdn
            });

            var url = baseUrl.TrimEnd('/') + endpointPath;
            var result = await SendWithRetries(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", "Bearer " + token);
                return request;
            }, baseUrl, appSettings);

            result.PurgedCdn = purgeCdn;
            return result;
        }

        private static async Task<CacheInvalidationInstanceResult> SendWithRetries(Func<HttpRequestMessage> requestFactory, string baseUrl, AppSettings appSettings)
        {
            var result = new CacheInvalidationInstanceResult { Url = baseUrl };
            var maxAttempts = Math.Max(1, GetMaxAttempts(appSettings));

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                result.Attempts = attempt;
                try
                {
                    using var request = requestFactory();
                    using var response = await Client.SendAsync(request);
                    result.StatusCode = (int)response.StatusCode;
                    // The frontend uses 207 when some cache layers could not be invalidated.
                    result.Success = response.IsSuccessStatusCode && result.StatusCode != 207;
                    result.Error = result.Success ? null : await response.Content.ReadAsStringAsync();

                    if (result.Success || !IsRetryable(result.StatusCode))
                        return result;
                }
                catch (System.Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                }

                if (attempt < maxAttempts)
                    await Task.Delay((int)(1000 * Math.Pow(3, attempt - 1)));
            }

            CMSLogger.Error($"[cache-invalidation] failed against {baseUrl}: {result.StatusCode} {result.Error}");
            return result;
        }

        internal static List<string> NormalizePaths(IEnumerable<string> paths)
        {
            var normalized = new List<string>();
            if (paths == null)
                return normalized;

            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                var value = path.Trim();
                if (HasPlaceholderSegment(value))
                    continue;

                if (!value.StartsWith("/"))
                    value = "/" + value;

                while (value.Contains("//"))
                    value = value.Replace("//", "/");

                if (value.Length > 1)
                    value = value.TrimEnd('/');

                if (!normalized.Contains(value, StringComparer.Ordinal))
                    normalized.Add(value);
            }

            return normalized;
        }

        private static List<string> GetFrontendInstances(AppSettings appSettings)
        {
            var instances = new List<string>();

            if (appSettings?.CacheInvalidation?.Nodes != null && appSettings.CacheInvalidation.Nodes.Count > 0)
                instances.AddRange(appSettings.CacheInvalidation.Nodes.Where(x => !string.IsNullOrWhiteSpace(x)));

            var envNodes = GetSetting("FrontEnd__CacheInvalidation__Nodes");
            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(envNodes))
                instances.AddRange(envNodes.Split('|'));

            if (instances.Count == 0 && appSettings?.CacheInvalidation?.Instances != null && appSettings.CacheInvalidation.Instances.Count > 0)
                instances.AddRange(appSettings.CacheInvalidation.Instances.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(appSettings?.CacheInvalidation?.Url))
                instances.Add(appSettings.CacheInvalidation.Url.Trim());

            var envInstances = GetSetting("FrontEnd__CacheInvalidation__Instances");
            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(envInstances))
                instances.AddRange(envInstances.Split('|'));

            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(GetSetting("FrontEnd__Url")))
                instances.Add(GetSetting("FrontEnd__Url").Trim());

            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(appSettings?.Content?.FrontendUrl))
                instances.Add(appSettings.Content.FrontendUrl.Trim());

            return instances
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string GetToken(AppSettings appSettings)
        {
            return GetSetting("FrontEnd__CacheInvalidation__Token")
                ?? appSettings?.CacheInvalidation?.Token;
        }

        private static string GetSecret(AppSettings appSettings)
        {
            var secret = GetSetting("FrontEnd__SecretCacheKey")
                ?? appSettings?.CacheInvalidation?.SecretCacheKey;
            return string.IsNullOrWhiteSpace(secret) ? "rinocms!" : secret;
        }

        private static int GetMaxAttempts(AppSettings appSettings)
        {
            if (appSettings?.CacheInvalidation?.MaxAttempts.HasValue == true)
                return appSettings.CacheInvalidation.MaxAttempts.Value;

            var raw = GetSetting("FrontEnd__CacheInvalidation__MaxAttempts");
            return int.TryParse(raw, out var parsed) ? parsed : 3;
        }

        private static bool HasPlaceholderSegment(string path)
        {
            return path.Split('/').Any(segment =>
                segment.Equals("undefined", StringComparison.OrdinalIgnoreCase) ||
                segment.Equals("null", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsRetryable(int statusCode)
        {
            return statusCode == 0 || statusCode == 207 || statusCode == 408 || statusCode == 429 || statusCode >= 500;
        }

        private static string GetSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }

    public enum CacheInvalidationMode
    {
        Batch,
        LegacyNext
    }

    public enum CacheInvalidationKind
    {
        News,
        Layout
    }

    public class CacheInvalidationResult
    {
        public CacheInvalidationResult()
        {
            Paths = new List<string>();
            Instances = new List<CacheInvalidationInstanceResult>();
        }

        public string Reason { get; set; }
        public string Mode { get; set; }
        public IList<string> Paths { get; set; }
        public IList<CacheInvalidationInstanceResult> Instances { get; set; }
        public long ElapsedMs { get; set; }
        public bool Success => Instances.Count > 0 && Instances.All(x => x.Success);
    }

    public class CacheInvalidationInstanceResult
    {
        public string Url { get; set; }
        public bool Success { get; set; }
        public bool PurgedCdn { get; set; }
        public int StatusCode { get; set; }
        public int Attempts { get; set; }
        public string Error { get; set; }
        public string UsedFallbackMode { get; set; }
    }
}
