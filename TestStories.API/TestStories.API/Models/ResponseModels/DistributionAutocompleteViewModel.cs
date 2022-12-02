using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class DistributionAutocompleteViewModel
    {
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

      
    }
}
