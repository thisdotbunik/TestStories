using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class BannerMediaModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }
    }
}
