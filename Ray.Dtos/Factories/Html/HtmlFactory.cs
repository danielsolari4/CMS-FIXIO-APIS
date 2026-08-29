using System.Linq;
using Ray.Dtos.Components;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos.Factories.Html
{

    public class HtmlFactory
    {
        private LayoutStructureDto Layout { get; set; }
        private JsonStructure JsonStructure { get; set; }
        private readonly string _imageUrl;
        public HtmlFactory(JsonStructure jsonStructure, LayoutStructureDto model, string imageUrl)
        {
            JsonStructure = jsonStructure;
            Layout = model;
            _imageUrl = imageUrl;
        }

        public string GetHtml()
        {
            var areas = string.Empty;
            foreach (var area in Layout.Areas)
            {
                var areasHtml = new HtmlFactory<AreaDto>(JsonStructure, area, _imageUrl).GetHtml();

                var regions = string.Empty;
                foreach (var region in area.Regions)
                {
                    var regionsHtml = new HtmlFactory<RegionDto>(JsonStructure, region, _imageUrl).GetHtml();

                    var components = string.Empty;
                    foreach (var component in region.Components)
                    {
                        components += new HtmlFactory<ComponentBaseDto>(JsonStructure, component, _imageUrl).GetHtml();
                    }
                    regionsHtml = regionsHtml.ReplaceByHtmlName("components", components);
                    regions += regionsHtml;
                }
                areasHtml = areasHtml.ReplaceByHtmlName("regions", regions);
                areas += areasHtml;
            }
            var layoutHtml = new HtmlFactory<LayoutStructureDto>(JsonStructure, Layout, _imageUrl).GetHtml();
            return layoutHtml.ReplaceByHtmlName("areas", areas);
        }
    }


    public class HtmlFactory<T> where T : class
    {
        private T Model { get; set; }
        private JsonStructure JsonStructure { get; set; }
        private string _imageUrl { get; set; }
        public HtmlFactory(JsonStructure jsonStructure, T model, string imageUrl)
        {
            JsonStructure = jsonStructure;
            Model = model;
            _imageUrl = imageUrl;
        }

        public string GetHtml()
        {

            if (typeof(T) == typeof(ComponentBaseDto))
            {

                var dto = Model as ComponentBaseDto;
                if (dto == null) return string.Empty;

                var structure = JsonStructure.Component.Types.FirstOrDefault(x => x.Type == dto.Type);

                var template = structure?.Html;
                var strcutureHtml = JsonStructure.Component.Html.ReplaceByHtmlName("component", template);

                return ((IGenerateHtml)Model)?.GenerateHtml(new HtmlBuilder(strcutureHtml, JsonStructure.Component?.HtmlTitle), _imageUrl);
            }

            if (typeof(T) == typeof(RegionDto))
            {
                var dto = Model as RegionDto;
                if (dto == null) return string.Empty;

                var structure = JsonStructure.Region;
                var template = structure?.Html;
                var titleHtml = structure?.HtmlTitle;


                return ((IGenerateHtml)Model)?.GenerateHtml(new HtmlBuilder(template, titleHtml), _imageUrl);
            }

            if (typeof(T) == typeof(AreaDto))
            {
                var dto = Model as AreaDto;
                if (dto == null) return string.Empty;

                var structure = JsonStructure.Area;
                var template = structure?.Html;
                var titleHtml = structure?.HtmlTitle;

                return ((IGenerateHtml)Model)?.GenerateHtml(new HtmlBuilder(template, titleHtml), _imageUrl);
            }

            if (typeof(T) == typeof(LayoutStructureDto))
            {
                var dto = Model as LayoutStructureDto;
                if (dto == null) return string.Empty;

                var structure = JsonStructure.Layout;
                var template = structure?.Html;

                return ((IGenerateHtml)Model)?.GenerateHtml(new HtmlBuilder(template), _imageUrl);
            }

            return string.Empty;
        }
    }

    public static class HtmlHelper
    {
        public static string ReplaceByPropertyName(this string html, string propertyName, string textreplace)
        {
            return html
                .Replace("{{" + propertyName + "}}", textreplace)
                .Replace("{{ " + propertyName + " }}", textreplace)
                .Replace("{{" + propertyName + " }}", textreplace)
                .Replace("{{ " + propertyName + "}}", textreplace);
        }
        public static string ReplaceByHtmlName(this string html, string propertyName, string textreplace)
        {
            return html
                .Replace("[{" + propertyName + "}]", textreplace)
                .Replace("[{ " + propertyName + " }]", textreplace)
                .Replace("[{ " + propertyName + "}]", textreplace)
                .Replace("[{" + propertyName + " }]", textreplace);
        }
    }

}
