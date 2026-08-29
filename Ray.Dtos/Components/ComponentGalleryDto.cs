using Ray.Dtos.Factories.Html;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos.Components
{
    public class ComponentGalleryDto : ComponentBaseDto, IComponentGallery
    {
        public string Title { get; set; }
        public int GalleryId { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;

            html = html.ReplaceByPropertyName("component.Title", Title);
            html = html.ReplaceByPropertyName("component.GalleryId", GalleryId.ToString());
            html = html.ReplaceByPropertyName("component.css", Css);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }

    }

}
