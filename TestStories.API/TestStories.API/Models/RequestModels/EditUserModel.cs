using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TestStories.API.Validators;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class EditUserModel
    {
        [JsonProperty(propertyName:"id")]
        [Required]
        public int Id { get; set; }

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
        public string Phone { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "apiKey")]
        [ApiKey(16)]
        public string ApiKey { get; set; }
    }
}
