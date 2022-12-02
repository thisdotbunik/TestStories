using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class User
    {
        public User()
        {
            Experiment = new HashSet<Experiment>();
            Favorites = new HashSet<Favorites>();
            MediaPublishUser = new HashSet<Media>();
            MediaUploadUser = new HashSet<Media>();
            Playlist = new HashSet<Playlist>();
            SubscriptionSeries = new HashSet<SubscriptionSeries>();
            SubscriptionTopic = new HashSet<SubscriptionTopic>();
            WatchHistory = new HashSet<WatchHistory>();
        }

        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public int? PartnerId { get; set; }
        public byte UsertypeId { get; set; }
        public byte UserstatusId { get; set; }
        public DateTime DateCreatedUtc { get; set; }
        public string SocialLoginId { get; set; }

        public virtual UserStatus Userstatus { get; set; }
        public virtual UserType Usertype { get; set; }
        public virtual ICollection<Experiment> Experiment { get; set; }
        public virtual ICollection<Favorites> Favorites { get; set; }
        public virtual ICollection<Media> MediaPublishUser { get; set; }
        public virtual ICollection<Media> MediaUploadUser { get; set; }
        public virtual ICollection<Playlist> Playlist { get; set; }
        public virtual ICollection<SubscriptionSeries> SubscriptionSeries { get; set; }
        public virtual ICollection<SubscriptionTopic> SubscriptionTopic { get; set; }
        public virtual ICollection<WatchHistory> WatchHistory { get; set; }
    }
}
