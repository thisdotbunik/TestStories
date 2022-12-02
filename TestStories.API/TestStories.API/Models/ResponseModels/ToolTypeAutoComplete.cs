using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ToolTypeAutoComplete
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
