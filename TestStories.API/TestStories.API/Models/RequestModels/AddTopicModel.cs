using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    /// <summary>
    /// Add Topic Model
    /// </summary>
    public class AddTopicModel
    {
        [JsonProperty(propertyName: "parentId")]
        public int? ParentId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "topicName")]
        [Required]
        public string TopicName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public IFormFile Logo { get; set; }
    }
}
