using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class PartnerModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }

        [JsonProperty(propertyName: "link")]
        public string Link { get; set; }

        [JsonProperty(propertyName:"orderNumber")]
        public int OrderNumber { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool ShowOnPartner { get; set; }
    }
}
