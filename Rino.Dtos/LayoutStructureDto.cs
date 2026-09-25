using Newtonsoft.Json;
using Rino.Dtos.Factories.Html;
using Rino.Dtos.Interfaces;

namespace Rino.Dtos
{
    //public class RootDto
    //{
    //    [JsonProperty(PropertyName = "Layout")]
    //    public LayoutStructureDto Layout { get; set; }

    //}

    public class LayoutStructureDto: IGenerateHtml
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "layoutTypeId")]
        public int LayoutTypeId { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        [JsonProperty(PropertyName = "areasCount")]
        public int AreasCount { get; set; }

        [JsonProperty(PropertyName = "areas")]
        public AreaDto[] Areas { get; set; }

        public string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            return builder.Html.ReplaceByPropertyName("layout.class", Css);
        }
    }

}
