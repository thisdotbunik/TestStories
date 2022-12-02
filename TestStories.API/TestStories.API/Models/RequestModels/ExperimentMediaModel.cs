using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class ExperimentMediaModel
    {
        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "mediaName")]
        public string MediaName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "titleCardImage")]
        public string TitleCardImage { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "cardImageUuid")]
        public string CardImageUuid { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName:"videoWatch")]
        public int VideoWatch { get; set; }

        [JsonProperty(propertyName:"videoShare")]
        public int VideoShare { get; set; }

        [JsonProperty(propertyName: "toolClicks")]
        public int ToolClicks { get; set; }

        [JsonProperty(propertyName:"vW25")]
        public int VW25 { get; set; }

        [JsonProperty(propertyName: "vW50")]
        public int VW50 { get; set; }

        [JsonProperty(propertyName: "vW75")]
        public int VW75 { get; set; }

        [JsonProperty(propertyName: "vW100")]
        public int VW100 { get; set; }
    }
}
