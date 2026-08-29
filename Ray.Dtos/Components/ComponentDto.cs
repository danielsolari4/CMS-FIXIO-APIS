using Newtonsoft.Json;
using Ray.Dtos.Factories;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos.Components
{
    public enum ComponentType
    {
        New = 1,
        DataFactory = 2,
        Video = 3,
        Widget = 4,
        LastNews = 5,
        RankingNews = 6,
        FrontCoverPrintEdition = 7,
        FeaturedNews = 8,
        Gallery = 9,
        Section = 10,
        Shorts = 11
    }


    [JsonConverter(typeof(ComponentCreationConverter))]
    public abstract class ComponentBaseDto : IGenerateHtml
    {
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }

        [JsonProperty(PropertyName = "CategoryId")]
        public int CategoryId { get; set; }

        [JsonProperty(PropertyName = "badge")]
        public BadgeDto Badge { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "fontColor")]
        public string FontColor { get; set; }
        public abstract string GenerateHtml(HtmlBuilder html, string imageUrl);
    }


    public class BadgeDto : IComponentBackgroundHeaderProperties
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        public string Link { get; set; }
        public string LabelLink { get; set; }
        public string Css { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }

    }
}
