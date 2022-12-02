using TestStories.DataAccess.Enums;

namespace TestStories.API.Models.RequestModels
{
    /// <summary>
    /// Signed Url Request Object
    /// </summary>
    public class VideoPipelineSignedUrlRequest
    {

        /// <summary>
        /// Video file type
        /// Current format supported by Video Pipeline are:
        /// "mov", "mp4", "m4v", "webm", "ogv", "mpg", "mpeg"
        /// </summary>
        public VideoFileTypeEnum FileType { get; set;  }

        /// <summary>
        /// Video FileName
        /// </summary>
        public string FileName { get; set;  }


        /// <summary>
        /// Video File Size
        /// </summary>
        public uint FileSize { get; set;  }

        /// <summary>
        /// Video File Description
        /// </summary>
        public string FileDescription { get; set;  }
    }
}
