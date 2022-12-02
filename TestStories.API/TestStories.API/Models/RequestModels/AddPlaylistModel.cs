using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddPlaylistModel
    {
        [JsonProperty(propertyName:"userId")]
        [Required]
        public int UserId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName:"name")]
        [Required]
        public string Name { get; set; }
    }
}
