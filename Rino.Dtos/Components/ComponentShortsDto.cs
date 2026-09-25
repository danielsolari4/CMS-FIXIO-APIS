using Newtonsoft.Json;
using Rino.Dtos.Factories.Html;
using Rino.Dtos.Interfaces;

namespace Rino.Dtos.Components
{
    public class ComponentShortsDto : ComponentBaseDto, IComponentHeaderProperties
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }

        [JsonProperty(PropertyName = "labelLink")]
        public string LabelLink { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html.ReplaceByPropertyName("component.css", Css);
            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");

            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }
    }
}
