using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddMediaModel
    {
        [JsonProperty(propertyName: "mediaTypeId")]
        [Required]
        public byte MediaTypeId { get; set; }

        [JsonProperty(propertyName: "mediaStatusId")]
        [Required]
        public byte MediaStatusId { get; set; }

        [JsonProperty(propertyName: "sourceId")]
        public int? SourceId { get; set; }

        [JsonProperty(propertyName: "title")]
        [Required]
        public string Title { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "longDescription")]
        public string LongDescription { get; set; }

        [JsonProperty(propertyName: "embeddedCode")]
        public string EmbeddedCode { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(propertyName: "mediaMetaData")]
        public string MediaMetaData { get; set; }

        [JsonProperty(propertyName: "isVisibleOnGoogle")]
        public bool IsVisibleOnGoogle { get; set; } = true;
    }
}
