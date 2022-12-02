using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class MediaPlayListModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "playlistId")]
        public int PlaylistId { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }
    }
}
