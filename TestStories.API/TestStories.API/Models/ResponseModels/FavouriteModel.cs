using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class FavouriteModel
    {

        [JsonProperty(propertyName: "favoriteId")]
        public int FavoriteId { get; set; }

        [JsonProperty(propertyName: "userId")]
        public int UserId { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }


    }
}
