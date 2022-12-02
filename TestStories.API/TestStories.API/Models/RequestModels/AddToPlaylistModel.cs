using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddToPlaylistModel
    {
        [JsonProperty(propertyName: "playlistId")]
        [Required]
        public int PlaylistId { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        [Required]
        public long MediaId { get; set; }
    }
}
