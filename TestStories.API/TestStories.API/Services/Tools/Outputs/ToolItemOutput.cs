using System;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public class ToolItemOutput
    {
        public int Id { get; set; }

        public string ToolName { get; set; }

        public string Title { get; set; }

        public DateTime DateCreated { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public string FeaturedImage { get; set; }

        public Images FeaturedImages { get; set; } = new Images();
    }
}
