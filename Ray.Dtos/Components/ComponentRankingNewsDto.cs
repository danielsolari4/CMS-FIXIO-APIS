using Ray.Dtos.Factories.Html;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos.Components
{
    public class ComponentRankingNewsDto : ComponentBaseDto, IComponentRankingNews
    {
        public string Title { get; set; }

        public int NodeId { get; set; }
        public string Css { get; set; }
        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;

            html = html.ReplaceByPropertyName("component.Title", Title);
            //html = html.ReplaceByPropertyName("component.NodeId", NodeId.ToString());
            html = html.ReplaceByPropertyName("component.css", Css);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }

    }

}
