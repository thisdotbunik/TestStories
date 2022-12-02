using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class UserAutoCompleteModel
    {
        [JsonProperty(propertyName: "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(propertyName: "lastName")]
        public string LastName { get; set; }

        [JsonProperty(propertyName: "company")]
        public string Company { get; set; }
    }

    public class UserAutoCompleteSearch
    {
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }
}
