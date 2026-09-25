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
    public static class WidgetHelper
    {
        public static List<int> GetWidgetIds(LayoutStructureDto model)
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
                        if (!(component is IComponentWidget)) continue;
                        var it = component as IComponentWidget;
                        if (it.Id != 0)
                        {
                            ids.Add(it.Id);
                        }
                    }
                }
            }
            return ids;
        }
        public static async Task<List<WidgetDto>> GetWidgets(int[] ids, SolrConfig solrConfig)
        {
            if (ids == null || !ids.Any()) return new List<WidgetDto>();

            try
            {
                var qs = "q=Id:(" + string.Join(" OR ", ids) + ") AND IsDeleted:false";
                var news = await SolrHelper.ExecuteQuery(SolrCore.WIDGET, qs + "&rows=" + ids.Count() + "&fl=Id,Name,Html,CreationDate" + "&no-pace", solrConfig);
                SolrResponse response = JsonConvert.DeserializeObject<SolrResponse>(news.ToString());
                var parse = JsonConvert.DeserializeObject<List<WidgetDto>>(JsonConvert.SerializeObject(response.response.docs));
                return parse;
            }
            catch (Exception e)
            {
                return new List<WidgetDto>();
            }
        }
    }
}
