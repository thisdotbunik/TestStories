using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ToolViewModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        [Required]
        public string Name { get; set; }

        [JsonProperty(propertyName:"toolTypeId")]
        public int? ToolTypeId { get; set; }

        [JsonProperty(propertyName:"partnerId")]
        public int? PartnerId { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "lstSeries")]
        public List<ShortSeriesModel> LstSeries { get; set; }

        [JsonProperty(propertyName: "medias")]
        public List<MediaShortModel> Medias { get; set; }

        [JsonProperty(propertyName: "link")]
        public string Link { get; set; }

        [JsonProperty(propertyName: "showOnMenu")]
        public bool ShowOnMenu { get; set; }

        [JsonProperty(propertyName: "showOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [JsonProperty(propertyName: "topics")]
        public List<ShortTopicModel> Topics { get; set; }

        [JsonProperty(propertyName: "assignto")]
        public AssignTo AssignTo { get; set; }

        [JsonProperty(propertyName: "assigntoItems")]
        public List<string> AssignToItems { get; set; }

        [JsonProperty(propertyName: "featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty(propertyName: "featuredImages")]
        public Images FeaturedImages { get; set; } = new Images();

        [JsonProperty(propertyName: "featuredImageFileName")]
        public string FeaturedImageFileName { get; set; }

        [JsonProperty(propertyName: "media")]
        public List<string> Media { get; set; }

        [JsonProperty(propertyName: "series")]
        public List<string> Series { get; set; }

        [JsonProperty(propertyName: "topicNames")]
        public List<string> TopicNames { get; set; }

        [JsonProperty(propertyName: "dateCreated")]
        public DateTime? DateCreated { get; set; }
    }

    public class AssignTo
    {
        [JsonProperty(propertyName: "media")]
        public List<string> Media { get; set; }

        [JsonProperty(propertyName: "series")]
        public List<string> Series { get; set; }
    }
}
