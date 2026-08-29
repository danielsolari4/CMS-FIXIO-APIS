using Newtonsoft.Json;

namespace Ray.Dtos.JsonStructures
{
    public class ComponentJsonStructure
    {
        [JsonProperty(PropertyName = "Types")]
        public ComponentTypeJsonStructure[] Types { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "HtmlTitle")]
        public string HtmlTitle { get; set; }

        [JsonProperty(PropertyName = "Html")]
        public string Html { get; set; }

        [JsonProperty(PropertyName = "Css")]
        public CssJsonStructure[] Css { get; set; }
    }

    public class ComponentTypeJsonStructure
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Html")]
        public string Html { get; set; }

        [JsonProperty(PropertyName = "Css")]
        public CssJsonStructure[] Css { get; set; }

        [JsonProperty(PropertyName = "Type")]
        public int Type { get; set; }

        [JsonProperty(PropertyName = "Structure")]
        public dynamic Structure { get; set; }
    }
}
