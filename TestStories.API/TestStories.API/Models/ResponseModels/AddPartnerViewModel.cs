using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class AddPartnerViewModel
    {

        [DefaultValue("")]
        [JsonProperty(propertyName: "name")]
        [Required]
        public string Name { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public IFormFile Logo { get; set; }

        [DefaultValue(false)]
        [JsonProperty(propertyName: "showOnPartnerPage")]
        public bool ShowOnPartnerPage { get; set; }

        [DefaultValue(false)]
        [JsonProperty(propertyName: "isArchived")]
        public bool IsArchived { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "partnerTypeIds")]
        public string PartnerTypeIds { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "link")]
        public string Link { get; set; }
    }

    public class EditPartnerViewModel
    {

        [DefaultValue("")]
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public IFormFile Logo { get; set; }

        [DefaultValue(false)]
        [JsonProperty(propertyName: "showOnPartnerPage")]
        public bool ShowOnPartnerPage { get; set; }

        [DefaultValue(false)]
        [JsonProperty(propertyName: "isArchived")]
        public bool IsArchived { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "partnerTypeIds")]
        public string PartnerTypeIds { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "logoFileName")]
        public string LogoFileName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "link")]
        public string Link { get; set; }
    }

}
