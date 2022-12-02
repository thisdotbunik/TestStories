using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class EditPlayListModel
    {
        [DefaultValue("")]
        [JsonProperty(propertyName: "playlistName")]
        [Required]
        public string PlaylistName { get; set; }

        [JsonProperty(propertyName: "updatedMediaIds")]
        public List<long> UpdatedMediaIds { get; set; }
    }
}
