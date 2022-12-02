using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ExperimentAutoComplete
    {
        [JsonProperty(propertyName: "name")]
        public string ExperimentName { get; set; }
    }
}
