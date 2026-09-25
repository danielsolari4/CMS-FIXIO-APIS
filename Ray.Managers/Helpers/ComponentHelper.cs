using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ray.Dtos;
using Ray.Dtos.Components;
using Ray.Dtos.Configuration;
using Ray.Dtos.Interfaces;

namespace Ray.Managers.Helpers
{
    public static class ComponentHelper
    {
        public static async Task ParseComponentsAsync(this LayoutStructureDto structure, string imageUrl, SolrConfig config)
        {
            List<AssetSolrDto> assets = null;
            List<MediaDto> medias = null;
            List<WidgetDto> widgets = null;
            List<SectionDto> sections = null;

            try
            {
                var asset = Task.Run(async () =>
                {
                    //get news ids & assets news
                    var newsIds = AssetHelper.GetNewsIds(structure);
                    assets = await AssetHelper.GetNews(newsIds.ToArray(), config);
                });
                var media = Task.Run(async () =>
                {
                    //get media ids & assets medias
                    var mediasIds = AssetHelper.GetMediaIds(structure);
                    medias = await AssetHelper.GetMedias(mediasIds.ToArray(), config);
                });
                var widget = Task.Run(async () =>
                {
                    //get media ids & assets medias
                    var widgetIds = WidgetHelper.GetWidgetIds(structure);
                    widgets = await WidgetHelper.GetWidgets(widgetIds.ToArray(), config);
                });

                var section = Task.Run(async () =>
                {
                    //get section components
                    var nodeIds = NodeHelper.GetNodesId(structure);
                    sections = await NodeHelper.GetNodes(nodeIds.ToArray(), config);
                });

                await Task.WhenAll(asset, media, widget, section);
            }
            finally
            {
                Parallel.ForEach(structure.Areas, areaDto =>
                {
                    if (areaDto.Regions == null) return;
                    Parallel.ForEach(areaDto.Regions, regionDto =>
                    {
                        ParseRegionComponents(regionDto, assets, medias, widgets, sections, imageUrl);
                    });
                });
            }
        }

        private static void ParseRegionComponents(RegionDto regionDto, List<AssetSolrDto> assets, List<MediaDto> medias, List<WidgetDto> widgets, List<SectionDto> sections, string imageUrl)
        {
            var components = regionDto.Components ?? new ComponentBaseDto[0];

            foreach (var componentNew in components.OfType<IComponentNew>())
            {
                componentNew.Parse(assets?.FirstOrDefault(x => x.Id == componentNew.Id), imageUrl);
            }

            foreach (var componentNew in components.OfType<IComponentVideo>())
            {
                componentNew.Parse(medias?.FirstOrDefault(x => x.Id == componentNew.Id), imageUrl);
            }

            foreach (var componentNew in components.OfType<IComponentWidget>())
            {
                componentNew.Parse(widgets?.FirstOrDefault(x => x.Id == componentNew.Id));
            }
            foreach (var componentNew in components.OfType<IComponentSection>())
            {
                componentNew.Parse(sections?.FirstOrDefault(x => x.NodeId == componentNew.NodeId), imageUrl);
            }

            if (regionDto.Regions != null)
            {
                foreach (var subRegion in regionDto.Regions)
                {
                    ParseRegionComponents(subRegion, assets, medias, widgets, sections, imageUrl);
                }
            }
        }


    }
}
