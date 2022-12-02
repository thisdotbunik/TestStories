using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class UserData
    {
        [JsonProperty("playlists")]
        public List<UserPlayListsModel> Playlists { get; set; }

        [JsonProperty("favorites")]
        public List<UserFavoritesModel> Favorites { get; set; }

        [JsonProperty("subscriptions")]
        public List<UserSubscriptionModel> Subscriptions { get; set; }

        [JsonProperty("watchHistory")]
        public List<UserWatchHistoryModel> WatchHistory { get; set; }
    }
}
