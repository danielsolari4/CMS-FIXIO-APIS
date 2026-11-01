using Rino.Dtos.Interfaces;
using Rino.Dtos.Factories.Html;

namespace Rino.Dtos.Components
{
    /// <summary>Componente "Hoy en Costa Rica" (tipo 12). Ver ComponentInstanceTodayInCostaRicaJson.</summary>
    public class ComponentTodayInCostaRicaDto : ComponentBaseDto
    {
        public string RestrictionFirst { get; set; }
        public string RestrictionSecond { get; set; }
        public string FuelSuper { get; set; }
        public string FuelRegular { get; set; }
        public string FuelDiesel { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;

            html = html.ReplaceByPropertyName("component.RestrictionFirst", RestrictionFirst);
            html = html.ReplaceByPropertyName("component.RestrictionSecond", RestrictionSecond);
            html = html.ReplaceByPropertyName("component.FuelSuper", FuelSuper);
            html = html.ReplaceByPropertyName("component.FuelRegular", FuelRegular);
            html = html.ReplaceByPropertyName("component.FuelDiesel", FuelDiesel);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }
    }
}
