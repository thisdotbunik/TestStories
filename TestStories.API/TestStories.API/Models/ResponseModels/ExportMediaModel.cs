using System;

namespace TestStories.API.Models.ResponseModels
{
    public class ExportMediaModel
    {
        public long MediaId { get; set; }
        public string MediaTitle { get; set; }
        public string ShortDesc { get; set; }
        public string FeaturedImage { get; set; }
        public string LongDesc { get; set; }
        public string Topic { get; set; }
        public string LinkedResources { get; set; }
        public string MediaType { get; set; }
        public string MediaStatus { get; set; }
        public string PublishedBy { get; set; }
        public DateTime? PublishDate { get; set; }
        public string SeriesId { get; set; }
        public string SeriesTitle { get; set; }
        public string Source { get; set; }
        public string DateCreated { get; set; }
        public string UploadedBy { get; set; }
        public string IsPrivate { get; set; }
        public string ActiveDateFrom { get; set; }
        public string ActiveDateTo { get; set; }
        public string Tags { get; set; }
        public string UploadedFileName { get; set; }
        public string SeoFriendlyUrl { get; set; }
        public string UniqueId { get; set; }

    }
}
