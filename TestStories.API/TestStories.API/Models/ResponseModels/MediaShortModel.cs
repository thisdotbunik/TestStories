using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class MediaShortModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "mediaTypeId")]
        public int MediaTypeId { get; set; }

        [JsonProperty(propertyName: "isActive")]
        public bool IsActive { get; set; }

        [JsonProperty(propertyName: "isVisibleOnGoogle")]
        public bool IsVisibleOnGoogle { get; set; }
    }
}
