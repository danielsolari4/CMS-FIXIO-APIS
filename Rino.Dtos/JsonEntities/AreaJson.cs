using Newtonsoft.Json;

namespace Rino.Dtos.JsonEntities
{
    public class AreaJson
    {
        [JsonProperty(PropertyName = "regions")]
        public RegionJson[] Regions { get; set; }

        [JsonProperty(PropertyName = "css")]
        public string Css { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }


        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }


        [JsonProperty(PropertyName = "labelLink")]
        public string LabelLink { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "fontColor")]
        public string FontColor { get; set; }

        [JsonProperty(PropertyName = "thematic")]
        public string Thematic { get; set; }

        [JsonProperty(PropertyName = "cssTitle")]
        public string CssTitle { get; set; }

        [JsonProperty(PropertyName = "colorContraste")]
        public string ColorContraste { get; set; }
    }
}
