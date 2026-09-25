using Rino.Dtos.Factories.Html;

namespace Rino.Dtos.Interfaces
{
    public interface IGenerateHtml
    {
        string GenerateHtml(HtmlBuilder html, string imageUrl);
    }

    public class HtmlBuilder
    {
        public string Html { get; set; }
        private string TitleHtml { get; set; }

        public HtmlBuilder(string html)
        {
            Html = html;
        }

        public HtmlBuilder(string html, string titleHtml)
        {
            Html = html;
            TitleHtml = titleHtml;
        }


        public string GetParseTitleHtml(IComponentHeaderProperties header, string type)
        {
            if(header == null) return string.Empty;
            if (string.IsNullOrEmpty(header.Title))
                return string.Empty;

            TitleHtml = TitleHtml.ReplaceByPropertyName(type + ".css", header.Css);
            TitleHtml = TitleHtml.ReplaceByPropertyName(type + ".title", header.Title);
            TitleHtml = TitleHtml.ReplaceByPropertyName(type + ".link", header.Link);
            TitleHtml = TitleHtml.ReplaceByPropertyName(type + ".labelLink", header.LabelLink);

            var dto = header as IComponentBackgroundHeaderProperties;
            if (dto == null) return TitleHtml;


            TitleHtml = TitleHtml.ReplaceByPropertyName(type + ".backgroundColor", dto?.BackgroundColor);

            return TitleHtml;
        }
    }
}
