using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class UserLoginRequest
    {
        [Required]
        [JsonProperty("username")]
        public string Username { get; set; }

        [Required]
        [JsonProperty("password")]
        public string Password { get; set; }

    }
}
