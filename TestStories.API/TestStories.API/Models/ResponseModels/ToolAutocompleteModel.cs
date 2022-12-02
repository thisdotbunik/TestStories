using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ToolAutocompleteModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string ToolName { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "link")]
        public string Link { get; set; }
    }
}
