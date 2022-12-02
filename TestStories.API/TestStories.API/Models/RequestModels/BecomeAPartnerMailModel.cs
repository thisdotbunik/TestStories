using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class BecomeAPartnerMailModel : GoogleReCaptchaModel
    {
        [DefaultValue("")]
        [JsonProperty(propertyName: "company")]
        [Required]
        public string Company { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "name")]
        [Required]
        public string Name { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "email")]
        [Required]
        public string Email { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "phone")]
        public string Phone { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "partnershipType")]
        public string PartnershipType { get; set; }

        [DefaultValue("")]
        [MaxLength(400, ErrorMessage = " Message Length Should be Maximum 400")]
        [JsonProperty(propertyName: "message")]
        public string Message { get; set; }
    }
}
