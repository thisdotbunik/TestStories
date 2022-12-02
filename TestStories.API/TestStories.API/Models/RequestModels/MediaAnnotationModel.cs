using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class MediaAnnotationModel
    {
        [JsonProperty(propertyName:"timeStamp")]
        public string  TimeStamp { get; set; }

        [JsonProperty(propertyName: "duration")]
        public int Duration { get; set; }

        [JsonProperty(propertyName: "text")]
        public string Text { get; set; }

        [JsonProperty(propertyName: "typeId")]
        public int TypeId { get; set; }

        [JsonProperty(propertyName: "resourceId")]
        public int? ResourceId { get; set; }

        [JsonProperty(propertyName: "link")]
        public string Link { get; set; }
    }
}
