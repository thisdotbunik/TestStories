using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class PartnerAutoCompleteSerachViewModel
    {
        [JsonProperty(propertyName: "name")]
        public string PartnerName { get; set; }
    }
}
