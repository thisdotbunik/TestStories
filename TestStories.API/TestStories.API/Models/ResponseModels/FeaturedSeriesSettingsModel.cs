using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class FeaturedSeriesSettingsModel
    {
        [JsonProperty(propertyName: "randomize")]
        [DefaultValue(true)]
        public bool Randomize { get; set; }

        [JsonProperty(propertyName: "setByAdmin")]
        [DefaultValue(false)]
        public bool SetByAdmin { get; set; }

        [JsonProperty(propertyName: "ids")]
        [DefaultValue(new int[0])]
        public ICollection<int> Ids { get; set; }
    }
}
