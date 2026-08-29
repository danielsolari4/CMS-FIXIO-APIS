using Newtonsoft.Json;

namespace Ray.Dtos.JsonStructures
{
    public class RegionJsonStructure
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Html")]
        public string Html { get; set; }
        [JsonProperty(PropertyName = "HtmlTitle")]
        public string HtmlTitle { get; set; }

        [JsonProperty(PropertyName = "Css")]
        public CssJsonStructure[] Css { get; set; }

        [JsonProperty(PropertyName = "Structure")]
        public dynamic Structure { get; set; }
    }

}
