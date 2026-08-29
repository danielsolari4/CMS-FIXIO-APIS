using Ray.Dtos.Factories.Html;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos.Components
{
    public class ComponentWidgetDto : ComponentBaseDto, IComponentWidget
    {
        public string Html { get; set; }
        public int WidgetTypeId { get; set; }
      

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;

            html = html.ReplaceByPropertyName("component.Id", Id.ToString());
            html = html.ReplaceByPropertyName("component.properties.Html", Html);
            html = html.ReplaceByPropertyName("component.properties.Html", Html);
            html = html.ReplaceByPropertyName("component.properties.Name", Name);
            html = html.ReplaceByPropertyName("component.properties.WidgetTypeId", WidgetTypeId.ToString());
            html = html.Replace("compile='component.properties.Html'", string.Empty);
            html = html.ReplaceByPropertyName("component.css", Css);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }


        public void Parse(WidgetDto widget)
        {
            if (widget == null) return;

            Id = widget.Id;
            Name = widget.Name;
            Html = widget.Html;
        }
    }

}
