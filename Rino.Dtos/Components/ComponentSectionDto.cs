using Rino.Dtos.Factories.Html;
using Rino.Dtos.Interfaces;

namespace Rino.Dtos.Components
{
    public class ComponentSectionDto : ComponentBaseDto, IComponentSection
    {
        public int NodeId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public MediaSizesPaths Media { get; set; }
        public string MediaUrl { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;

            html = html.ReplaceByPropertyName("Id", Id.ToString());
            html = html.ReplaceByPropertyName("Url", Url);
            html = html.ReplaceByPropertyName("component.Title", Title);
            html = html.ReplaceByPropertyName("mediaurl", MediaUrl);
            html = html.ReplaceByPropertyName("component.css", Css);
            html = html.ReplaceByPropertyName("component.Media.Size2Path", Media?.Size2Path);
            html = html.ReplaceByPropertyName("component.Media.Size5Path", Media?.Size5Path);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");

            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }

        public void Parse(SectionDto section, string imageUrl)
        {
            if (section == null) return;

            Title = section.Title;
            Url = section.Url;

            if (section.Media != null)
                Media = new MediaSizesPaths
                {
                    Size2Path = section.Media?.Size2Path,
                    Size5Path = section.Media?.Size5Path
                };

            MediaUrl = imageUrl;
        }
    }
}
