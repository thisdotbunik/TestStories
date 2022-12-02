using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class EditTopicModel
    {
        [JsonProperty(propertyName: "id")]
        [Required]
        public int Id { get; set; }

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

        [DefaultValue("")]
        [JsonProperty(propertyName: "logoFileName")]
        public string LogoFileName { get; set; }
    }
}
