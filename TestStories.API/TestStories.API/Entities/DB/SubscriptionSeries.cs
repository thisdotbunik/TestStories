using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class SubscriptionSeries
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SeriesId { get; set; }

        public virtual Series Series { get; set; }
        public virtual User User { get; set; }
    }
}
