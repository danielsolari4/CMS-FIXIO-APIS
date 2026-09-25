using Newtonsoft.Json;
using Rino.Dtos.Factories;

namespace Rino.Dtos.JsonEntities
{
    public class LayoutInstanceJson
    {
        [JsonProperty(PropertyName = "layoutTypeId")]
        public int LayoutTypeId { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        [JsonProperty(PropertyName = "areasCount")]
        public int AreasCount { get; set; }

        [JsonProperty(PropertyName = "areas")]
        public AreaInstanceJson[] Areas { get; set; }

    }

    public class AreaInstanceJson
    {
        [JsonProperty(PropertyName = "regions")]
        public RegionInstanceJson[] Regions { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }

        [JsonProperty(PropertyName = "labelLink")]
        public string LabelLink { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }
        [JsonProperty(PropertyName = "fontColor")]
        public string FontColor { get; set; }

        [JsonProperty(PropertyName = "thematic")]
        public string Thematic { get; set; }
        [JsonProperty(PropertyName = "cssTitle")]
        public string CssTitle { get; set; }

        [JsonProperty(PropertyName = "colorContraste")]
        public string ColorContraste { get; set; }
    }

    public class RegionInstanceJson
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }


        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }


        [JsonProperty(PropertyName = "labelLink")]
        public string LabelLink { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        [JsonProperty(PropertyName = "distribution")]
        public string Distribution { get; set; }

        [JsonProperty(PropertyName = "components")]
        public ComponentInstanceBaseJson[] Components { get; set; }

        [JsonProperty(PropertyName = "regions")]
        public RegionInstanceJson[] Regions { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "fontColor")]
        public string FontColor { get; set; }

        [JsonProperty(PropertyName = "thematic")]
        public string Thematic { get; set; }
        [JsonProperty(PropertyName = "cssTitle")]
        public string CssTitle { get; set; }

        [JsonProperty(PropertyName = "colorContraste")]
        public string ColorContraste { get; set; }
    }

    [JsonConverter(typeof(ComponentInstanceCreationConverter))]
    public abstract class ComponentInstanceBaseJson
    {
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        [JsonProperty(PropertyName = "layoutSize")]
        public string LayoutSize { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }
        [JsonProperty(PropertyName = "fontColor")]
        public string FontColor { get; set; }

        [JsonProperty(PropertyName = "badge")]
        public BadgeInstanceBaseJson Badge { get; set; }
    }

    public class BadgeInstanceBaseJson
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }
    }

    public class ComponentInstanceNewJson : ComponentInstanceBaseJson
    {
        [JsonProperty(PropertyName = "MediaDiscriminator")]
        public string MediaDiscriminator { get; set; }

        //[JsonProperty(PropertyName = "NewId")]
        //public int NewId { get; set; }
        //[JsonProperty(PropertyName = "NewsId")]
        //public int[] NewsId { get; set; } 
    }

    public class ComponentInstanceVideoJson : ComponentInstanceBaseJson
    {
    }

    public class ComponentInstanceWidgetJson : ComponentInstanceBaseJson
    {
        //[JsonProperty(PropertyName = "MediaId")]
        //public int WidgetId { get; set; }
    }

    public class ComponentInstanceDataFactoryJson : ComponentInstanceBaseJson
    {
    }

    public class ComponentInstanceShortsJson : ComponentInstanceBaseJson
    {
    }

    public class ComponentInstanceLastNewsJson : ComponentInstanceBaseJson
    {
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Css { get; set; }
        public int NodeId { get; set; }
        public int CountNews { get; set; }
    }

    public class ComponentInstanceRankingNewsJson : ComponentInstanceBaseJson
    {
        public string Title { get; set; }
        public string Css { get; set; }
        public int NodeId { get; set; }
    }

    public class ComponentInstanceSectionJson : ComponentInstanceBaseJson
    {
        public int NodeId { get; set; }
    }


    public class ComponentFrontCoverPrintEditionJson : ComponentInstanceBaseJson
    {
        public string Link { get; set; }

        public string Image { get; set; }

        public string AutomaticClass { get; set; }
        public string NodeId { get; set; }
    }

    public class ComponentInstanceFeaturedNewsJson : ComponentInstanceBaseJson
    {
        public int AuthorId { get; set; }

        public int NodeId { get; set; }
    }

    public class ComponentInstanceGalleryJson : ComponentInstanceBaseJson
    {
        public string Title { get; set; }

        public int GalleryId { get; set; }
    }
}
