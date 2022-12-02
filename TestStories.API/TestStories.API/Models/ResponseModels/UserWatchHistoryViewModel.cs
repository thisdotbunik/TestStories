using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class UserWatchHistoryViewModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "mediaName")]
        public string MediaName { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(propertyName: "thumbnails")]
        public Images Thumbnails { get; set; } = new Images();

        [JsonProperty(propertyName: "featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty(propertyName: "featuredImages")]
        public Images FeaturedImages { get; set; } = new Images();

        [JsonProperty(propertyName: "mediaTypeId")]
        public int MediaTypeId { get; set; }

        [JsonProperty(propertyName: "isSharingAllowed")]
        public bool? IsSharingAllowed { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty(propertyName: "seriesId")]
        public int? SeriesId { get; set; }

        [JsonProperty(propertyName: "series")]
        public string Series { get; set; }
    }
}
