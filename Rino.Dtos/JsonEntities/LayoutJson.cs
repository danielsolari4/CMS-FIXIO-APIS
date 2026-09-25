using Newtonsoft.Json;

namespace Rino.Dtos.JsonEntities
{
    public class LayoutJson
    {

        [JsonProperty(PropertyName = "layoutTypeId")]
        public int LayoutTypeId { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        [JsonProperty(PropertyName = "areasCount")]
        public int AreasCount { get; set; }

        [JsonProperty(PropertyName = "areas")]
        public AreaJson[] Areas { get; set; }
    }
}
