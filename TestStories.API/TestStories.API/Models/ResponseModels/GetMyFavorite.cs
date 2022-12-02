using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class GetMyFavorite
    {
        [JsonProperty(propertyName: "medianame")]
        public string MediaName { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }
    }
}
