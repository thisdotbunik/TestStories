using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class MediaDetailsTopicWise
    {
        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(propertyName: "url")]
        public string URL { get; set; }
    }
}
