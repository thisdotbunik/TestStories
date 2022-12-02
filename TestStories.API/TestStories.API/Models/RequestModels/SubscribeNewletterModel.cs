using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SubscribeNewletterModel
    {
        [DefaultValue("")]
        [JsonProperty(propertyName:"name")]
        [Required]
        public string Name { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName:"email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DefaultValue("homepage")]
        [JsonProperty(propertyName: "source")]
        public string Source { get; set; }
    }
}
