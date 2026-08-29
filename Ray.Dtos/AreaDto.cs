
using Newtonsoft.Json;
using Ray.Dtos.Factories.Html;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos
{
    public class AreaDto : IGenerateHtml, IComponentHeaderProperties
    {
        [JsonProperty(PropertyName = "regions")]
        public RegionDto[] Regions { get; set; }

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


        public string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            if (!string.IsNullOrEmpty(ColorContraste))
                Css += " " + ColorContraste;

            if (!string.IsNullOrEmpty(CssTitle))
                Css += " " + CssTitle;

            var html = builder.Html.ReplaceByPropertyName("area.css", Css)
               .ReplaceByPropertyName("area.thematic", !string.IsNullOrEmpty(BackgroundColor) ? "area--tematica" : "")
               .ReplaceByPropertyName("area.backgroundColor", BackgroundColor);
            var titleSection = builder.GetParseTitleHtml(this, "area");

            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }
    }
}
