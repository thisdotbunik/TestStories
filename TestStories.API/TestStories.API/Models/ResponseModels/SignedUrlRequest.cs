using Amazon.S3;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class SignedUrlModel
    {
        /// <summary>
        /// Pre-Signed URI
        /// </summary>
        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        /// <summary>
        /// Uuid
        /// </summary>
        [JsonProperty(propertyName: "uuid")]
        public string Uuid { get; set; }


        /// <summary>
        /// Thumbnail Uuid
        /// </summary>
        [JsonProperty(propertyName: "thumbnailUuid")]
        public string ThumbnailUuid { get; set; }
        /// <summary>
        /// Http Verb that is supported by this service
        /// </summary>
        [JsonProperty(propertyName: "httpVerb")]
        public HttpVerb HttpVerb { get; set; }
    }
}
