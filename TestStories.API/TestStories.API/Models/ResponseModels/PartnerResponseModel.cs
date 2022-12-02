using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class PartnerResponseModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "isArchived")]
        public bool IsArchived { get; set; }

        [JsonProperty(propertyName: "showOnPartner")]
        public bool ShowOnPartnerPage { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }
    }
}
