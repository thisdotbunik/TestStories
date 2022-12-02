using System;
using System.Collections.Generic;
using TestStories.API.Models.RequestModels;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public partial class MediaViewModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "longdescription")]
        public string LongDescription { get; set; }

        [JsonProperty(propertyName: "topic")]
        public List<string> Topic { get; set; }

        //[JsonProperty(propertyName: "topicId")]
        //public int TopicId { get; set; }

        [JsonProperty(propertyName: "mediaType")]
        public string MediaType { get; set; }

        [JsonProperty(propertyName: "mediaTypeId")]
        public int MediaTypeId { get; set; }

        [JsonProperty(propertyName: "mediaStatus")]
        public string MediaStatus { get; set; }

        [JsonProperty(propertyName: "publishedById")]
        public int? PublishedById { get; set; }

        [JsonProperty(propertyName: "publishedBy")]
        public string PublishedBy { get; set; }

        [JsonProperty(propertyName: "series")]
        public string Series { get; set; }

        [JsonProperty(propertyName: "seriesId")]
        public int SeriesId { get; set; }

        [JsonProperty(propertyName: "source")]
        public string Source { get; set; }

        [JsonProperty(propertyName: "sourceId")]
        public int SourceId { get; set; }

        [JsonProperty(propertyName: "publishDate")]
        public DateTime? PublishDate { get; set; }

        [JsonProperty(propertyName: "createdDate")]
        public DateTime? CreatedDate { get; set; }

        [JsonProperty(propertyName: "uploadedById")]
        public int? UploadedById { get; set; }

        [JsonProperty(propertyName: "uploadedByUser")]
        public string UploadedByUser { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }

        [JsonProperty(propertyName: "logos")]
        public Images Logos { get; set; } = new Images();

        [JsonProperty(propertyName: "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(propertyName: "thumbnails")]
        public Images Thumbnails { get; set; } = new Images(); 

        [JsonProperty(propertyName: "featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty(propertyName: "featuredImages")]
        public Images FeaturedImages { get; set; } = new Images();

        [JsonProperty(propertyName: "embeddedCode")]
        public string EmbeddedCode { get; set; }

        [JsonProperty(propertyName: "isPrivate")]
        public bool IsPrivate { get; set; }

        [JsonProperty(propertyName: "isSharingAllowed")]
        public bool IsSharingAllowed { get; set; }

        [JsonProperty(propertyName: "activeFromUtc")]
        public DateTime? ActiveFromUtc { get; set; }

        [JsonProperty(propertyName: "activeToUtc")]
        public DateTime? ActiveToUtc { get; set; }

        [JsonProperty(propertyName: "tags")]
        public List<string> Tags { get; set; }

        [JsonProperty(propertyName: "tool")]
        public ShortToolModel Tool { get; set; }

        [JsonProperty(propertyName: "tools")]
        public List<ShortToolModel> Tools { get; set; }

        [JsonProperty(propertyName: "mediaAnnotations")]
        public List<MediaAnnotationModel> MediaAnnotations { get; set; }

        [JsonProperty(propertyName: "mediaTools")]
        public List<string> MediaTools { get; set; }

        [JsonProperty(propertyName: "imageFileName")]
        public string ImageFileName { get; set; }

        [JsonProperty(propertyName: "mediaMetaData")]
        public string MediaMetaData { get; set; }

        [JsonProperty(propertyName: "srtFile")]
        public string SrtFile { get; set; }

        [JsonProperty(propertyName: "srtFileName")]
        public string SrtFileName { get; set; }

        [JsonProperty(propertyName: "lstSrtFile")]
        public List<SrtFileModel> LstSrtFile { get; set; }

        [JsonProperty(propertyName: "topicIds")]
        public List<int> TopicIds { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "draftMediaSeoUrl")]
        public string DraftMediaSeoUrl { get; set; }

        [JsonProperty(propertyName: "lastUpdatedDate")]
        public DateTime? LastUpdatedDate { get; set; }

        [JsonProperty(propertyName: "uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty(propertyName:"isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty(propertyName: "isVisibleOnGoogle")]
        public bool IsVisibleOnGoogle { get; set; }

    }
}
