using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class SrtFileModel
    {
        [JsonProperty(propertyName: "srtFile")]
        public string SrtFile { get; set; }

        [JsonProperty(propertyName: "srtFileName")]
        public string SrtFileName { get; set; }

        [JsonProperty(propertyName: "srtLanguage")]
        public string SrtLanguage { get; set; }

        [JsonProperty(propertyName: "uUid")]
        public string Uuid { get; set; }

        [JsonProperty(propertyName: "preSignedUrl")]
        public string PreSignedUrl { get; set; }
    }
}
