namespace TestStories.API.Models.RequestModels
{
    /// <summary>
    /// Signed Url Request Object
    /// </summary>
    public class VideoPipelineEventError
    {

        /// <summary>
        /// Video file Id
        /// UUID format
        /// </summary>
        public string Uuid { get; set;  }

        /// <summary>
        /// Error message e.g.
        /// why video is not transcoded
        /// </summary>
        public string Message { get; set;  }
    }
}
