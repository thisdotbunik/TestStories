using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Services.Validators;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class GoogleReCaptchaModel
    {
        [Required]
        [GoogleReCaptchaValidator]
        [JsonProperty(propertyName: "g-recaptcha-response")]
        [BindProperty(Name = "g-recaptcha-response")]
        public string GoogleReCaptchaResponse { get; set; }
    }
}
