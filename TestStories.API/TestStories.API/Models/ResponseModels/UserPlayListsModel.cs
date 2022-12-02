using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class UserPlayListsModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mediaThumbnail")]
        public string MediaThumbnail { get; set; }

        [JsonProperty("medias")]
        public List<UserPlaylistMedia> Medias { get; set; }

        [JsonProperty("totalMedias")]
        public int TotalMedias { get; set; }
    }
}
