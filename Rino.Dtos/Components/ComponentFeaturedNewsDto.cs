using Rino.Dtos.Factories.Html;
using Rino.Dtos.Interfaces;

namespace Rino.Dtos.Components
{
    public class ComponentFeaturedNewsDto : ComponentBaseDto, IComponentFeaturedNews
    {
        public int NodeId { get; set; }
        public int AuthorId { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;

            html = html.ReplaceByPropertyName("component.NodeId", NodeId.ToString());
            html = html.ReplaceByPropertyName("component.AuthorId", AuthorId.ToString());
            html = html.ReplaceByPropertyName("component.css", Css);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }

    }

}
