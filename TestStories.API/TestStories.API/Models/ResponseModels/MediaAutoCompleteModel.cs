using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class MediaAutoCompleteModel
    {
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }
}
