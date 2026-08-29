using System.Linq;
using Ray.Dtos.Factories.Html;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos.Components
{
    public class ComponentVideoDto : ComponentBaseDto, IComponentVideo
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public MediaSizesPaths MediaSizesPaths { get; set; }
        public string MediaUrl { get; set; }
        public string NodeContentTitle { get; set; }
        public string Discriminator { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;
          
            html = html.ReplaceByPropertyName("component.properties.NodeContentTitleTheme", NodeContentTitle?.ToLower().Replace(" ", "-"));
            html = html.ReplaceByPropertyName("component.properties.NodeContentTitle", NodeContentTitle);

            html = html.ReplaceByPropertyName("component.properties.Id", Id.ToString());
            html = html.ReplaceByPropertyName("component.Id", Id.ToString());
            html = html.ReplaceByPropertyName("Url", Url);
            html = html.ReplaceByPropertyName("component.properties.MediaSizesPaths.AssetMediaSizePath2", MediaSizesPaths?.Size2Path);
            html = html.ReplaceByPropertyName("component.properties.Title", Title);
            html = html.ReplaceByPropertyName("mediaurl", MediaUrl);
            html = html.ReplaceByPropertyName("component.css", Css);
            html = html.ReplaceByPropertyName("component.properties.Discriminator", Discriminator);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }


        public void Parse(MediaDto asset, string imageUrl)
        {
            if (asset == null) return;

            Id = asset.Id;
            Title = asset.Title;
            Url = asset.MediaUrl;
            MediaSizesPaths = asset.SizesPaths;
            MediaUrl = imageUrl;
            NodeContentTitle = (asset.CategoriesName != null && asset.CategoriesName.Any()) ? asset.CategoriesName?[0] : string.Empty;
            Discriminator = asset.Discriminator;
        }
    }
}
