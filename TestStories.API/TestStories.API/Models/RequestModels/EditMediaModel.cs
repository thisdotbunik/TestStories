using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class EditMediaModel
    {
        [JsonProperty(propertyName: "id")]
        [Required]
        public long Id { get; set; }

        [JsonProperty(propertyName: "title")]
        [Required]
        public string Title { get; set; }

        [JsonProperty(propertyName: "mediaStatusId")]
        [Required]
        public byte MediaStatusId { get; set; }

        [JsonProperty(propertyName: "resourceIds")]
        public string ResourceIds { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "LongDescription")]
        public string LongDescription { get; set; }

        [JsonProperty(propertyName: "seriesId")]
        public int? SeriesId { get; set; }

        [JsonProperty(propertyName: "topicIds")]
        public string TopicIds { get; set; }

        [JsonProperty(propertyName: "tags")]
        public string Tags { get; set; }

        [JsonProperty(propertyName: "sourceId")]
        public int? SourceId { get; set; }

        [JsonProperty(propertyName: "isPrivate")]
        public bool IsPrivate { get; set; }

        [JsonProperty(propertyName: "isSharingAllowed")]
        public bool IsSharingAllowed { get; set; }

        [JsonProperty(propertyName: "activeFromUtc")]
        public string ActiveFromUtc { get; set; }

        [JsonProperty(propertyName: "activeToUtc")]
        public string ActiveToUtc { get; set; }

        [JsonProperty(propertyName: "publishDate")]
        public string PublishDate { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(propertyName: "embeddedCode")]
        public string EmbeddedCode { get; set; }

        [JsonProperty(propertyName: "featuredImage")]
        public IFormFile FeaturedImage { get; set; }

        [JsonProperty(propertyName: "imageFileName")]
        public string ImageFileName { get; set; }

        [JsonProperty(propertyName: "mediaMetaData")]
        public string MediaMetaData { get; set; }

        [JsonProperty(propertyName: "srtFileName")]
        public List<string> SrtFileNames { get; set; }

        [JsonProperty(propertyName: "mediaAnnotations")]
        public string MediaAnnotations { get; set; }

        [JsonProperty(propertyName: "lstSrt")]
        public string LstSrt { get; set; }

        [JsonProperty(propertyName: "draftMediaSeoUrl")]
        public string DraftMediaSeoUrl { get; set; }

        [JsonProperty(propertyName: "uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty(propertyName: "isVisibleOnGoogle")]
        public bool IsVisibleOnGoogle { get; set; }
    }

    public class MediaSrtItem
    {
        [JsonProperty(propertyName: "file")]
        public string File { get; set; }

        [JsonProperty(propertyName: "fileMetaData")]
        public string FileMetaData { get; set; }

        [JsonProperty(propertyName: "language")]
        public string Language { get; set; }

        [JsonProperty(propertyName: "isAdd")]
        public bool IsAdd { get; set; }

        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
    }
}
