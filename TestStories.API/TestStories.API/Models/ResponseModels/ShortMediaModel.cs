using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ShortMediaModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "mediaTitle")]
        public string MediaTitle { get; set; }

        [JsonProperty(propertyName: "uniqueId")]
        public string UniqueId { get; set; }
    }
}
