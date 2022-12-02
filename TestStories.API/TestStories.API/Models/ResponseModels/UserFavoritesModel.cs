using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class UserFavoritesModel:UserPlaylistMedia
    {
        [JsonProperty("mediaId")]
        public long MediaId { get; set; }

        [JsonProperty("mediaName")]
        public string MediaName { get; set; }
    }
}
