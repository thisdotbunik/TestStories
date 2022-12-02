using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SubscribeTopic
    {
        [JsonProperty(propertyName: "userId")]
        [Required]
        public int UserId { get; set; }

        [JsonProperty(propertyName: "topicId")]
        [Required]
        public int TopicId { get; set; }
    }
}
