using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class PartnerMediaModel
    {
        [JsonProperty(propertyName:"partnerId")]
        public int PartnerId { get; set; }

        [JsonProperty(propertyName:"mediaId")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "email")]
        public string Email { get; set; }

    }
}
