using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class EditorPicks
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string EmbeddedCode { get; set; }
    }
}
