using System;

namespace TestStories.API.Models.ResponseModels
{
    public class ExportUserDataModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string NewsletterSubscribed { get; set; }
        public string SignupDateUtc { get; set; }
        public DateTime? LastLoginDateUtc { get; set; }
        public string Playlists { get; set; }
        public string SubscribedSeries { get; set; }
        public string SubscribedTopics { get; set; }
        public string Favorites { get; set; }
    }
}
