using System;
using Amazon.DynamoDBv2.DataModel;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Models
{
    /// <summary>
    ///
    /// </summary>
    [DynamoDBTable("video-metadata", LowerCamelCaseProperties=true)]
    public class VideoMetadata
    {
        /// <summary>
        ///
        /// </summary>
        [DynamoDBHashKey]
        public string Uuid { get; set; }
        /// <summary>
        /// Time to leave. e.g. Expiration time
        /// </summary>
        [DynamoDBProperty(StoreAsEpoch = true)]
        public DateTime Ttl { get; set; }
        /// <summary>
        ///
        /// </summary>
        [DynamoDBProperty]
        public Metadata Meta { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DynamoDBProperty]
        public VideoPipelineStatusEnum Status { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $@"{Uuid} - {Ttl}. Status: {Status} .Metadata: {Meta}";
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class Metadata
    {
        /// <summary>
        ///
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        ///
        /// </summary>
        public VideoFileTypeEnum FileType { get; set; }
        /// <summary>
        ///
        /// </summary>
        public uint FileSize { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string FileDescription { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string FilePathOriginal { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string FilePathTranscoded { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string FilePathThumbnail { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string BucketOriginal { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string BucketTranscoded { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $@"{FileName} - {FileType}";
        }
    }
}
