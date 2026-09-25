using Newtonsoft.Json;

namespace Rino.Dtos.JsonStructures
{
    public class LayoutJsonStructure
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Html")]
        public string Html { get; set; }

        [JsonProperty(PropertyName = "Css")]
        public string[] Css { get; set; }

    }
}
