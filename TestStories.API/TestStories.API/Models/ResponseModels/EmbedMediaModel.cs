using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class EmbedMediaModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "embedCode")]
        public string EmbedCode { get; set; }
    }
}
