using Newtonsoft.Json;
using Ray.Dtos.Components;
using Ray.Dtos.Factories.Html;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos
{

    public class RegionDto : IGenerateHtml, IComponentHeaderProperties
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

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "fontColor")]
        public string FontColor { get; set; }

        [JsonProperty(PropertyName = "thematic")]
        public string Thematic { get; set; }

        [JsonProperty(PropertyName = "components")]
        public ComponentBaseDto[] Components { get; set; }
        [JsonProperty(PropertyName = "cssTitle")]
        public string CssTitle { get; set; }

        [JsonProperty(PropertyName = "colorContraste")]
        public string ColorContraste { get; set; }

        public string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            if (!string.IsNullOrEmpty(ColorContraste))
                Css += " " + ColorContraste;
            if (!string.IsNullOrEmpty(CssTitle))
                Css += " " + CssTitle;

            var html = builder.Html.ReplaceByPropertyName("region.css", Css)
                .ReplaceByPropertyName("region.thematic", !string.IsNullOrEmpty(BackgroundColor) ? "region--tematica" : "")
                .ReplaceByPropertyName("region.distribution", Distribution)
                .ReplaceByPropertyName("region.backgroundColor", BackgroundColor);
            var titleSection = builder.GetParseTitleHtml(this, "region");

            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }
    }
}
