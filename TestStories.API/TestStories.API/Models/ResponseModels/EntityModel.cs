using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class EntityModel
    {
        [JsonProperty(propertyName:"name")]
        public string Name { get; set; }

        [JsonProperty(propertyName:"count")]
        public int Count { get; set; }
    }
}
