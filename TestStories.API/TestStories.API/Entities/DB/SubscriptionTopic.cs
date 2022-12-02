using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class SubscriptionTopic
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TopicId { get; set; }

        public virtual Topic Topic { get; set; }
        public virtual User User { get; set; }
    }
}
