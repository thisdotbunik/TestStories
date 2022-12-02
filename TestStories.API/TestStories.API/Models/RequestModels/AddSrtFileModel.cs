using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddSrtFileModel
    {
        [DefaultValue("")]
        [JsonProperty(propertyName: "file")]
        public string File { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "fileMetadata")]
        public string FileMetadata { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "language")]
        public string Language { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }
    }
}
