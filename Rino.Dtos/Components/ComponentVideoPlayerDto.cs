using Rino.Dtos.Factories.Html;
using Rino.Dtos.Interfaces;

namespace Rino.Dtos.Components
{
    /// <summary>
    /// Componente "Reproductor de videos" (tipo 13): un video principal y la cola de los siguientes, tomados de una galería multimedia.
    /// Ver ComponentInstanceVideoPlayerJson.
    /// </summary>
    public class ComponentVideoPlayerDto : ComponentBaseDto, IComponentGallery
    {
        public string Title { get; set; }
        public int GalleryId { get; set; }
        /// <summary>Muestra la etiqueta "EN VIVO" sobre el reproductor (la dibuja el sitio; la vista previa de los admin no).</summary>
        public bool IsLive { get; set; }

        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;

            html = html.ReplaceByPropertyName("component.Title", Title);
            html = html.ReplaceByPropertyName("component.GalleryId", GalleryId.ToString());

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");
            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }
    }
}
