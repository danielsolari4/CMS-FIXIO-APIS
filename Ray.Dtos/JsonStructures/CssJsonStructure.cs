using Newtonsoft.Json;

namespace Ray.Dtos.JsonStructures
{
    public class CssJsonStructure
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }
    }
}
