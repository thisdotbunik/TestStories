using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;


namespace TestStories.API.Models.RequestModels
{
    public class ContactUsMailModel : GoogleReCaptchaModel
    {
        [DefaultValue("")]
        [JsonProperty(propertyName:"name")]
        [Required]
        public string Name { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName:"email")]
        [Required]
        public string Email { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "phone")]
        public string Phone { get; set; }

        [DefaultValue("")]
        [MaxLength(1000, ErrorMessage =" Message Length Should be Maximum 1000")]
        [JsonProperty(propertyName: "message")]
        public string Message { get; set; }
    }
}
