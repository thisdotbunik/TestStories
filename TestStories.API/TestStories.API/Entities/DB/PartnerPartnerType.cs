using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class PartnerPartnerType
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public byte PartnertypeId { get; set; }

        public virtual Partner Partner { get; set; }
        public virtual PartnerType Partnertype { get; set; }
    }
}
