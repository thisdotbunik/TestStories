using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class PartnerType
    {
        public PartnerType()
        {
            PartnerPartnerType = new HashSet<PartnerPartnerType>();
        }

        public byte Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<PartnerPartnerType> PartnerPartnerType { get; set; }
    }
}
