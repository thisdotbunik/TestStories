using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddToolType
    {
        [DefaultValue("")]
        [JsonProperty("name")]
        [Required]
        public string Name { get; set; }
    }
}
