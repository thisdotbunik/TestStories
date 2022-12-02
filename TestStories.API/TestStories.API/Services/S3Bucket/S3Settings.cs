namespace TestStories.API.Infrastructure
{
    /// <summary>
    /// Should not be used. Or at least a name we should change.
    /// </summary>
    public class S3Settings
    {
        /// <summary>
        /// S3 bucket name
        /// </summary>
        public string S3MediaBucketName { get; set; }

        /// <summary>
        /// Presigned URL expiration time
        /// </summary>
        public string S3MediaQueueLinkExpirationInMinutes { get; set; }

        /// <summary>
        /// ConcurrentServiceRequests
        /// </summary>
        public string ConcurrentServiceRequests { get; set; }

        /// <summary>
        /// MinSizeBeforePartUpload
        /// </summary>
        public string MinSizeBeforePartUpload { get; set; }

        /// <summary>
        /// PartSize
        /// </summary>
        public string PartSize { get; set; }
    }
}
