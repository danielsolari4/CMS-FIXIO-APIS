using Ray.Dtos.Factories.Html;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos.Components
{
    public class ComponentFrontCoverPrintEditionDto : ComponentBaseDto, IComponentFrontCoverPrintEdition
    {
        public string Image { get; set; }
        public string Link { get; set; }
        
        public string MediaUrl { get; set; }
        public string AutomaticClass { get; set; }
        public string NodeId { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;
            MediaUrl = imageUrl;


            html = html.ReplaceByPropertyName("component.properties.Link", Link);
            html = html.ReplaceByPropertyName("component.properties.Image", Image);
            html = html.ReplaceByPropertyName("mediaurl", MediaUrl);
            html = html.ReplaceByPropertyName("component.css", Css);

            html = html.ReplaceByPropertyName("component.properties.AutomaticClass", AutomaticClass);
            html = html.ReplaceByPropertyName("component.properties.NodeId", NodeId?.ToString());

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }
    }
}
