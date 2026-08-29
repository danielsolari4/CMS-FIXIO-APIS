using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ray.Dtos;
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
                    Parallel.ForEach(areaDto.Regions, regionDto =>
                    {
                        foreach (var componentNew in regionDto.Components.OfType<IComponentNew>())
                        {
                            componentNew.Parse(assets?.FirstOrDefault(x => x.Id == componentNew.Id), imageUrl);
                        }

                        foreach (var componentNew in regionDto.Components.OfType<IComponentVideo>())
                        {
                            componentNew.Parse(medias?.FirstOrDefault(x => x.Id == componentNew.Id), imageUrl);
                        }

                        foreach (var componentNew in regionDto.Components.OfType<IComponentWidget>())
                        {
                            componentNew.Parse(widgets?.FirstOrDefault(x => x.Id == componentNew.Id));
                        }
                        foreach (var componentNew in regionDto.Components.OfType<IComponentSection>())
                        {
                            componentNew.Parse(sections?.FirstOrDefault(x => x.NodeId == componentNew.NodeId), imageUrl);
                        }
                    });
                });
            }




        }

    }
}
