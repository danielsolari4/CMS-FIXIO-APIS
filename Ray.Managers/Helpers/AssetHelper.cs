using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Dtos.Interfaces;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers.Helpers
{
    public static class AssetHelper
    {
        public static List<int> GetNewsIds(LayoutStructureDto model)
        {
            var ids = new List<int>();
            if (model?.Areas == null) return ids;
            foreach (var area in model?.Areas)
            {
                if (area.Regions == null) continue;
                foreach (var region in area.Regions)
                {
                    foreach (var component in LayoutStructureHelper.GetComponents(region))
                    {
                        if (!(component is IComponentNew)) continue;
                        var it = component as IComponentNew;
                        if (it.Id != 0)
                        {
                            ids.Add(it.Id);
                        }
                        if (it.NewsId != null && it.NewsId.Any())
                        {
                            ids.AddRange(it.NewsId.ToList());
                        }
                    }
                }
            }
            return ids;
        }

        public static List<int> GetMediaIds(LayoutStructureDto model)
        {
            var ids = new List<int>();
            if (model?.Areas == null) return ids;
            foreach (var area in model?.Areas)
            {
                if (area.Regions == null) continue;
                foreach (var region in area.Regions)
                {
                    foreach (var it in LayoutStructureHelper.GetComponents(region).OfType<IComponentVideo>())
                    {
                        if (it.Id != 0)
                        {
                            ids.Add(it.Id);
                        }
                    }
                }
            }
            return ids;
        }

        public static List<int> GetIds<T>(LayoutStructureDto model)
        {
            var ids = new List<int>();
            if (model?.Areas == null) return ids;
            foreach (var area in model?.Areas)
            {
                if (area.Regions == null) continue;
                foreach (var region in area.Regions)
                {
                    foreach (var component in LayoutStructureHelper.GetComponents(region))
                    {
                        if (typeof(T) == typeof(IComponentNew))
                        {
                            if (!(component is IComponentNew)) continue;
                            var it = component as IComponentNew;
                            if (it.Id != 0)
                            {
                                ids.Add(it.Id);
                            }
                            if (it.NewsId != null && it.NewsId.Any())
                            {
                                ids.AddRange(it.NewsId.ToList());
                            }
                        }

                        if (typeof(T) == typeof(IComponentVideo))
                        {
                            if (!(component is IComponentVideo)) continue;
                            var it = component as IComponentVideo;
                            if (it.Id != 0)
                            {
                                ids.Add(it.Id);
                            }
                        }
                    }
                }
            }
            return ids;
        }

        public static async Task<List<AssetSolrDto>> GetNews(int[] ids, SolrConfig config)
        {
            if (ids == null || !ids.Any()) return new List<AssetSolrDto>();

            try
            {
                var news = await SolrHelper.ExecuteQuery(SolrCore.NEWS, "q=Id:(" + string.Join(" OR ", ids) + ") AND IsDeleted:false AND IsPrivate:false AND Status:PUBLISHED&rows=" + ids.Count() + "&fl=Id,Url,Title_en,Nodes_en,Description_en,Nodes_slug,MediaDiscriminator,MediaSizesPaths:[json],PublicationDate,AuthorName,AuthorId,LastModificationDate", config);
                SolrResponse response = JsonConvert.DeserializeObject<SolrResponse>(news.ToString());
                var parse = JsonConvert.DeserializeObject<List<AssetSolrDto>>(JsonConvert.SerializeObject(response.response.docs));
                return parse;
            }
            catch (Exception e)
            {
                return new List<AssetSolrDto>();
            }
        }

        public static async Task<List<MediaDto>> GetMedias(int[] ids, SolrConfig config)
        {
            if (ids == null || !ids.Any()) return new List<MediaDto>();

            try
            {
                var media = await SolrHelper.ExecuteQuery(SolrCore.MEDIA, "q=Id:(" + string.Join(" OR ", ids) + ")&rows=" + ids.Count() + "&fl=Id,MediaUrl,MediaType,LastModificationDate,PublicationDate,Title,Categories_Name,SizesPaths:[json],Discriminator,Description&indent=on&q=IsEnabled:true&rows=100&sort=PublicationDate%20desc", config);
                SolrResponse response = JsonConvert.DeserializeObject<SolrResponse>(media.ToString());
                var parse = JsonConvert.DeserializeObject<List<MediaDto>>(JsonConvert.SerializeObject(response.response.docs));
                return parse;
            }
            catch (Exception e)
            {
                return new List<MediaDto>();
            }
        }
    }
}