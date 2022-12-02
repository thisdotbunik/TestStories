using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ShortTopicModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }
}
