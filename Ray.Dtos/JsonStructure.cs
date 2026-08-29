using Newtonsoft.Json;
using Ray.Dtos.JsonStructures;

namespace Ray.Dtos
{
    public class JsonStructure
    {
        [JsonProperty(PropertyName = "Layout")]
        public LayoutJsonStructure Layout { get; set; }

        [JsonProperty(PropertyName = "Area")]
        public AreaJsonStructure Area { get; set; }

        [JsonProperty(PropertyName = "Region")]
        public RegionJsonStructure Region { get; set; }

        [JsonProperty(PropertyName = "Component")]
        public ComponentJsonStructure Component { get; set; }
    }
}
