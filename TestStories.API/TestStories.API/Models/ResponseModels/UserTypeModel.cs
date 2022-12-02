using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class UserTypeModel
    {
        [JsonProperty(propertyName: "id")]
        public byte Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }
}
