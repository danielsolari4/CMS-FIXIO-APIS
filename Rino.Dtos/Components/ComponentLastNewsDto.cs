using Rino.Dtos.Factories.Html;
using Rino.Dtos.Interfaces;

namespace Rino.Dtos.Components
{
    public class ComponentLastNewsDto : ComponentBaseDto, IComponentLastNews
    {
        public string Title { get; set; }
        public int NodeId { get; set; }
        public string Keywords { get; set; }
        public int CountNews { get; set; }
        public string Css { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;
            
            html = html.ReplaceByPropertyName("component.Title", Title);
            html = html.ReplaceByPropertyName("component.NodeId", NodeId.ToString());
            html = html.ReplaceByPropertyName("component.CountNews", CountNews.ToString());
            html = html.ReplaceByPropertyName("component.Keywords", Keywords);
            html = html.ReplaceByPropertyName("component.Css", Css);
            html = html.ReplaceByPropertyName("component.css", string.Empty);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);

        }

    }

}
