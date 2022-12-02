using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class PartnerDetailViewModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "partnerTypeIds")]
        public List<byte> PartnerTypeIds { get; set; }

        [JsonProperty(propertyName: "partnerType")]
        public List<string> PartnerType { get; set; }

        [JsonProperty(propertyName: "dateAdded")]
        public DateTime DateAdded { get; set; }

        [JsonProperty(propertyName: "isArchived")]
        public bool IsArchived{ get; set; }

        [JsonProperty(propertyName: "showOnPartner")]
        public bool ShowOnPartnerPage { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }

        [JsonProperty(propertyName:"logoFileName")]
        public string LogoFileName { get; set; }

        [JsonProperty(propertyName: "link")]
        public string Link { get; set; }
    }
}
