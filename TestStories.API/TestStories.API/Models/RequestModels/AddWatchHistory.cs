using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddWatchHistory
    {
        [Required]
        [JsonProperty(propertyName: "userId")]
        public int UserId { get; set; }

        [Required]
        [JsonProperty(propertyName: "mediaid")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "createdDate")]
        public DateTime CreatedDate { get; set; }
    }
}
