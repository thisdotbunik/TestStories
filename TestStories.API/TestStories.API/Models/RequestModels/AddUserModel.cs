using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddUserModel
    {
        [JsonProperty(propertyName: "userTypeId")]
        [Required]
        public byte UserTypeId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "firstName")]
        [Required]
        public string FirstName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "lastName")]
        [Required]
        public string LastName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "email")]
        [Required]
        public string Email { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "phone")]
        [MaxLength(12)]
        public string Phone { get; set; }

        [JsonProperty(propertyName: "partnerId")]
        [DefaultValue(null)]
        public int? PartnerId { get; set; }
    }
}
