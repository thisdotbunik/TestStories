using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ToolTypeModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }
    }
}
