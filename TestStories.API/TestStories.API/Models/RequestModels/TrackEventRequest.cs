using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class TrackEventRequest
    {
        [JsonProperty(propertyName: "eventTypeId")]
        public int EventTypeId { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }
    }
}
