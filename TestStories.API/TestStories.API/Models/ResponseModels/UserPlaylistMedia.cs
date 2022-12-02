using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class UserPlaylistMedia
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }

    }
}
