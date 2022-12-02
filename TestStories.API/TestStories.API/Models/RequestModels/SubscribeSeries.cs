using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SubscribeSeries
    {
        [JsonProperty(propertyName:"userId")]
        [Required]
        public int UserId { get; set; }

        [JsonProperty(propertyName:"seriesId")]
        [Required]
        public int SeriesId { get; set; }
    }
}
