using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class MediaSeoDetailModel
    {
        [JsonProperty(propertyName:"id")]
        public long Id { get; set; }

        [JsonProperty(propertyName:"title")]
        public string Title { get; set; }

        [JsonProperty(propertyName:"oldSeoUrl")]
        public string OldSeoUrl { get; set; }

        [JsonProperty(propertyName:"newSeoUrl")]
        public string NewSeoUrl { get; set; }

        [JsonProperty(propertyName: "mediaMetaData")]
        public string MediaMetaData { get; set; }
    }
}
