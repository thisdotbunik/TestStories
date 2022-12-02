using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddEmbedMediaModel
    {
        [JsonProperty(propertyName: "title")]
        [Required]
        public string Title { get; set; }

        [JsonProperty(propertyName: "embedCode")]
        [Required]
        public string EmbedCode { get; set; }
    }
}
