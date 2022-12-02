using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class RegisterActiveUser
    {
        [JsonProperty(propertyName: "userTypeId")]
        [Required]
        [Range(1,5, ErrorMessage ="UserTypeId should be between 1 to 5")]
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
        [EmailAddress]
        //[RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$" , ErrorMessage = "+ is not  is not allowed in email address")]
        public string Email { get; set; }

        [Required]
        [StringLength(256 , MinimumLength = 8 , ErrorMessage = "Password must be between 8 and 256 characters in length")]
        [JsonProperty("password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
