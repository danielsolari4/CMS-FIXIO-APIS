using Newtonsoft.Json;

namespace Ray.Dtos.JsonStructures
{
    public class AreaJsonStructure
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Html")]
        public string Html { get; set; }

        [JsonProperty(PropertyName = "Css")]
        public string[] Css { get; set; }

        [JsonProperty(PropertyName = "HtmlTitle")]
        public string HtmlTitle { get; set; }

    }
}
