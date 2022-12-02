using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SignUpUserModel
    {
        [JsonProperty(propertyName: "firstName")]
        [Required]
        public string FirstName { get; set; }

        [JsonProperty(propertyName: "lastName")]
        [Required]
        public string LastName { get; set; }

        [JsonProperty(propertyName: "email")]
        [Required]
        public string Email { get; set; }

        [JsonProperty(propertyName: "password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
