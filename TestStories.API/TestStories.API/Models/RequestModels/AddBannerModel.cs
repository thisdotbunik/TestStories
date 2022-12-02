using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddBannerModel
    {
        [JsonProperty(propertyName: "title")]
        [Required]
        public string Title { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "image")]
        [Required]
        public IFormFile Image { get; set; }
    }
}
