using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class TopicAutoCompleteModel
    {
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }
    }
}
