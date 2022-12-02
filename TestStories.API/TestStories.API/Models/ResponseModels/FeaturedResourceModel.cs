using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class FeaturedResourceModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "partnerId")]
        public int? PartnerId { get; set; }

        public string Partner { get; set; }

        [JsonProperty(propertyName: "typeId")]
        public int? TypeId { get; set; }

        [JsonProperty(propertyName: "type")]
        public string Type { get; set; }

        [JsonProperty(propertyName: "topics")]
        public List<string> Topics { get; set; }

        [JsonProperty(propertyName: "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(propertyName: "thumbnails")]
        public Images Thumbnails { get; set; } = new Images();

        [JsonProperty(propertyName: "showOnMenu")]
        public bool ShowOnMenu { get; set; }

        [JsonProperty(propertyName: "showOnHomePage")]
        public bool ShowOnHomePage { get; set; }

    }
}
