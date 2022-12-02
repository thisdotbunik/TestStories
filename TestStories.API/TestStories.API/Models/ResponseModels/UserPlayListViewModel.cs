using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class playListItem
    {
        [JsonProperty(propertyName: "playlists")]
        public List<UserPlayListViewModel> Playlists { get; set; }

        [JsonProperty(propertyName: "playlistCount")]
        public int PlaylistCount { get; set; }
    }

    public class UserPlayListViewModel
    {
        [JsonProperty(propertyName: "playlistId")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "mediaThumbnail")]
        public string MediaThumbnail { get; set; }

        [JsonProperty(propertyName: "mediaThumbnails")]
        public Images MediaThumbnails { get; set; } = new Images();

        [JsonProperty(propertyName: "medias")]
        public List<MediaInfoModel> Medias { get; set; }

    }
}


public class videos
{
    [JsonProperty(propertyName: "id")]
    public long MediaId { get; set; }

    [JsonProperty(propertyName: "mediaThumbnail")]
    public string MediaThumbnail { get; set; }

    [JsonProperty(propertyName: "seoUrl")]
    public string SeoUrl { get; set; }
}






