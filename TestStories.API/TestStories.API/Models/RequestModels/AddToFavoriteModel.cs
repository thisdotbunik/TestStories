using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddToFavoriteModel
    {
        [JsonProperty(propertyName: "userId")]
        [Required]
        public int UserId { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        [Required]
        public long MediaId { get; set; }
    }
}
