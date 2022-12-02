using System.ComponentModel;

namespace TestStories.API.Models.RequestModels
{
    public class FilterMediaStandaloneModel
    {
        public string ApiKey { get; set; }

        [DefaultValue("all")]
        public string Fields { get;set; }

        [DefaultValue("all")]
        public string Ids { get; set; }

        /// <summary>
        /// Possible values: Video, PodcastAudio, EmbeddedMedia, Banner
        /// </summary>
        [DefaultValue("all")]
        public string MediaTypes { get; set; }
    }
}
