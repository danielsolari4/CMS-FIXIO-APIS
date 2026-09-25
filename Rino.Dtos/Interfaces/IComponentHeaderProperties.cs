using Newtonsoft.Json;

namespace Rino.Dtos.Interfaces
{
    public interface IComponentHeaderProperties
    {
        [JsonProperty(PropertyName = "title")]
        string Title { get; set; }

        [JsonProperty(PropertyName = "link")]
        string Link { get; set; }

        [JsonProperty(PropertyName = "labelLink")]
        string LabelLink { get; set; }

        [JsonProperty(PropertyName = "css")]
        string Css { get; set; }
    }
    public interface IComponentBackgroundHeaderProperties : IComponentHeaderProperties
    {
        [JsonProperty(PropertyName = "backgroundColor")]
        string BackgroundColor { get; set; }
    }
}