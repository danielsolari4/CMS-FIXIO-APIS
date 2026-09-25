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
    public static class NodeHelper
    {
        public static List<int> GetNodesId(LayoutStructureDto model)
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
                        if (!(component is IComponentSection)) continue;
                        var it = component as IComponentSection;
                        if (it.NodeId != 0)
                        {
                            ids.Add(it.NodeId);
                        }
                    }
                }
            }
            return ids;
        }
        public static async Task<List<SectionDto>> GetNodes(int[] ids, SolrConfig config)
        {
            if (ids == null || !ids.Any()) return new List<SectionDto>();

            try
            {
                var qs = "q=Id:(" + string.Join(" OR ", ids) + ") AND IsDeleted:false AND IsPublished:true";
                var nodes = await SolrHelper.ExecuteQuery(SolrCore.NODE, qs + "&rows=" + ids.Count() + "&fl=Id,Description,Title_en" + "&no-pace", config);

                var qsPage = "q=Nodes_id:(" + string.Join(" OR ", ids) + ") AND IsDeleted:false";
                var page = await SolrHelper.ExecuteQuery(SolrCore.PAGE, qsPage + "&rows=" + ids.Count() + "&no-pace", config);


                var result = new List<SectionDto>();
                foreach (dynamic it in ((dynamic)nodes).response.docs)
                {
                    var se = (dynamic)it;

                    var url = (string)se.Description;

                    if (!string.IsNullOrEmpty(url) && !url.StartsWith("/"))
                        url = "/" + url;

                    var section = new SectionDto
                    {
                        NodeId = (int)se.Id,
                        Title = (string)se.Title_en,
                        Url = url
                    };

                    foreach (dynamic media in ((dynamic)page).response.docs)
                    {
                        var nodeId = ((Newtonsoft.Json.Linq.JToken)((dynamic)media).Nodes_id).First();
                        if (nodeId == null || (int)nodeId != section.NodeId) continue;
                        if (media.MediaSizesPaths == null) continue;

                        var mediaProp = JsonConvert.DeserializeObject<MediaSizesPaths>(media.MediaSizesPaths.ToString());
                        if (mediaProp != null)
                            section.Media = mediaProp;

                        if (media.Media_id == null) continue;
                        var i = 0;
                        foreach (var mediaId in media.Media_id)
                        {
                            if (media.Featured_list == null) continue;

                            var isFeatured = (bool)media.Featured_list[i];
                            if (!isFeatured)
                            {
                                var listMedia = new List<int> { (int)mediaId };
                                var obj = await AssetHelper.GetMedias(listMedia.ToArray(), config);
                                var mediaParse = obj.FirstOrDefault();
                                if (mediaParse?.SizesPaths != null)
                                    section.Media = new MediaSizesPaths
                                    {
                                        Size1Path = mediaParse.SizesPaths.Size1Path,
                                        Size2Path = mediaParse.SizesPaths.Size2Path,
                                        Size3Path = mediaParse.SizesPaths.Size3Path,
                                        Size4Path = mediaParse.SizesPaths.Size4Path,
                                        Size5Path = mediaParse.SizesPaths.Size5Path
                                    };
                            }
                            i++;
                        }
                    }
                    
                  

                    result.Add(section);
                }

                return result;
            }
            catch (Exception e)
            {
                return new List<SectionDto>();
            }
        }
    }
}
