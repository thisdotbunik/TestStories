using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class EditToolModel
    {
        [JsonProperty(propertyName: "id")]
        [Required]
        public int Id { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "name")]
        [Required]
        public string Name { get; set; }

        [JsonProperty(propertyName: "toolTypeId")]
        [DefaultValue("")]
        public byte? ToolTypeId { get; set; }

        [JsonProperty(propertyName: "partnerId")]
        public int? PartnerId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "link")]
        [Required]
        public string Link { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "seriesIds")]
        public string SeriesIds { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "mediaIds")]
        public string MediaIds { get; set; }

        [JsonProperty(propertyName: "featuredImage")]
        public IFormFile FeaturedImage { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "featuredImageFileName")]
        public string FeaturedImageFileName { get; set; }

        [JsonProperty(propertyName: "showOnMenu")]
        public bool ShowOnMenu { get; set; }

        [JsonProperty(propertyName: "showOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "topicIds")]
        public string TopicIds { get; set; }
    }
}
