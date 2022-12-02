using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class Images
    {
        [JsonProperty(propertyName:"banner")]
        public string Banner { get; set; }

        [JsonProperty(propertyName:"grid")]
        public string Grid { get; set; }

        [JsonProperty(propertyName:"thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(propertyName: "smallThumbnail")]
        public string SmallThumbnail { get; set; }
    }
}
