using Newtonsoft.Json;

namespace Ray.Dtos.JsonEntities
{
    public class ThemeJson
    {
        [JsonProperty("mainLogo")]
        public string MainLogo { get; set; }

        [JsonProperty("mobileLogo")]
        public string MobileLogo { get; set; }

        [JsonProperty("firstColor")]
        public string FirstColor { get; set; }

        [JsonProperty("secondColor")]
        public string SecondColor { get; set; }

        [JsonProperty("thirdColor")]
        public string ThirdColor { get; set; }

        [JsonProperty("imgSource")]
        public string ImgSource { get; set; }

        [JsonProperty("sectionsColors")]
        public SectionsColor[] SectionsColors { get; set; }
    }

    public class SectionsColor
    {
        [JsonProperty("nodeId")]
        public int NodeId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }

    
}
