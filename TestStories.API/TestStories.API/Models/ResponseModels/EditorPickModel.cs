using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class EditorPickModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty("embeddedCode")]
        public string EmbeddedCode { get; set; }
    }
}
