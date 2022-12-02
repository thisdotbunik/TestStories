using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Setting
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
