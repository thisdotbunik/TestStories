using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SaveFeaturedTopicsSettingsModel
    {
        [JsonProperty(propertyName: "randomize")]
        [Required]
        [DefaultValue(true)]
        public bool Randomize { get; set; }

        [JsonProperty(propertyName: "setByAdmin")]
        [Required]
        [DefaultValue(false)]
        public bool SetByAdmin { get; set; }

        [JsonProperty(propertyName: "ids")]
        [DefaultValue(new int[0])]
        public ICollection<int> Ids { get; set; }
    }
}
