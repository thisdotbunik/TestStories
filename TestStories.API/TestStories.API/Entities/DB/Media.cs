using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Media
    {
        public Media()
        {
            ExperimentMedia = new HashSet<ExperimentMedia>();
            Favorites = new HashSet<Favorites>();
            MediaSrt = new HashSet<MediaSrt>();
            MediaTag = new HashSet<MediaTag>();
            MediaTopic = new HashSet<MediaTopic>();
            PartnerMedia = new HashSet<PartnerMedia>();
            PlaylistMedia = new HashSet<PlaylistMedia>();
            ToolMedia = new HashSet<ToolMedia>();
            WatchHistory = new HashSet<WatchHistory>();
        }

        public byte[] Rv { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte MediatypeId { get; set; }
        public byte MediastatusId { get; set; }
        public int UploadUserId { get; set; }
        public int? PublishUserId { get; set; }
        public int? SeriesId { get; set; }
        public int? TopicId { get; set; }
        public int? SourceId { get; set; }
        public string EmbeddedCode { get; set; }
        public string Url { get; set; }
        public DateTime DateCreatedUtc { get; set; }
        public DateTime? DatePublishedUtc { get; set; }
        public DateTime? ActiveFromUtc { get; set; }
        public DateTime? ActiveToUtc { get; set; }
        public bool IsPrivate { get; set; }
        public bool? IsSharingAllowed { get; set; }
        public string Thumbnail { get; set; }
        public string FeaturedImage { get; set; }
        public string Metadata { get; set; }
        public string FeaturedImageMetadata { get; set; }
        public string SrtFile { get; set; }
        public string SrtFileMetadata { get; set; }

        public virtual MediaStatus Mediastatus { get; set; }
        public virtual MediaType Mediatype { get; set; }
        public virtual User PublishUser { get; set; }
        public virtual Series Series { get; set; }
        public virtual Partner Source { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual User UploadUser { get; set; }
        public virtual ICollection<ExperimentMedia> ExperimentMedia { get; set; }
        public virtual ICollection<Favorites> Favorites { get; set; }
        public virtual ICollection<MediaSrt> MediaSrt { get; set; }
        public virtual ICollection<MediaTag> MediaTag { get; set; }
        public virtual ICollection<MediaTopic> MediaTopic { get; set; }
        public virtual ICollection<PartnerMedia> PartnerMedia { get; set; }
        public virtual ICollection<PlaylistMedia> PlaylistMedia { get; set; }
        public virtual ICollection<ToolMedia> ToolMedia { get; set; }
        public virtual ICollection<WatchHistory> WatchHistory { get; set; }
    }
}
