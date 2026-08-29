using Ray.Dtos.Components;
using Ray.Dtos.JsonEntities;

namespace Ray.Dtos.Factories
{
    public static class ComponentFactory
    {
        public static ComponentBaseDto GetInstance(int componentTypeId)
        {
            var type = (ComponentType)componentTypeId;
            switch (type)
            {
                case ComponentType.New: return new ComponentNewDto();
                case ComponentType.DataFactory: return new ComponentDataFactoryDto();
                case ComponentType.Video: return new ComponentVideoDto();
                case ComponentType.Widget: return new ComponentWidgetDto();
                case ComponentType.LastNews: return new ComponentLastNewsDto();
                case ComponentType.RankingNews: return new ComponentRankingNewsDto();
                case ComponentType.FrontCoverPrintEdition: return new ComponentFrontCoverPrintEditionDto();
                case ComponentType.FeaturedNews: return new ComponentFeaturedNewsDto();
                case ComponentType.Gallery: return new ComponentGalleryDto();
                case ComponentType.Section: return new ComponentSectionDto();
                default:
                    return null;
            }
        }

        public static ComponentInstanceBaseJson GetJsonInstance(int componentTypeId)
        {
            var type = (ComponentType)componentTypeId;
            switch (type)
            {
                case ComponentType.New: return new ComponentInstanceNewJson();
                case ComponentType.DataFactory: return new ComponentInstanceDataFactoryJson();
                case ComponentType.Video: return new ComponentInstanceVideoJson();
                case ComponentType.Widget: return new ComponentInstanceWidgetJson();
                case ComponentType.LastNews: return new ComponentInstanceLastNewsJson();
                case ComponentType.RankingNews: return new ComponentInstanceRankingNewsJson();
                case ComponentType.FrontCoverPrintEdition: return new ComponentFrontCoverPrintEditionJson();
                case ComponentType.FeaturedNews: return new ComponentInstanceFeaturedNewsJson();
                case ComponentType.Gallery: return new ComponentInstanceGalleryJson();
                case ComponentType.Section: return new ComponentInstanceSectionJson();
                default:
                    return null;
            }
        }
    }
}
