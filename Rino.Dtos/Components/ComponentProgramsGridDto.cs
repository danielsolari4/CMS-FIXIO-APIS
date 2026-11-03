using Rino.Dtos.Interfaces;
using Rino.Dtos.Factories.Html;

namespace Rino.Dtos.Components
{
    /// <summary>Componente "Programas" (tipo 14). Ver ComponentInstanceProgramsGridJson.</summary>
    public class ComponentProgramsGridDto : ComponentBaseDto
    {
        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return builder.Html.ReplaceByHtmlName("titleHtml", titleSection);
        }
    }
}
