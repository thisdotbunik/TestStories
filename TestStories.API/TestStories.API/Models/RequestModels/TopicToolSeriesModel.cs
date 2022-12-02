using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class TopicToolSeriesModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty("bannerImage")]
        public string BannerImage { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty("parentTopic")]
        public string ParentTopic { get; set; }

        [JsonProperty("assignedTo")]
        public List<string> AssignedTo { get; set; }

        [JsonProperty(propertyName: "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("topics")]
        public List<string> Topics { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("showOnMenu")]
        public int ShowOnMenu { get; set; }

        [JsonProperty("showOnHomePage")]
        public int ShowOnHomePage { get; set; }

        [JsonProperty("itemtype")]
        public string ItemType { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("partner")]
        public string Partner { get; set; }

        [JsonProperty(propertyName: "statusAddedOnCloud")]
        public string StatusAddedOnCloud { get; set; }
    }
}
