using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.CloudSearch.Service.Model;

namespace TestStories.API.Models.ResponseModels
{
    public class AllInOne
    {
        public List<OptimisedBlogResponse> FeaturedBlogs { get; set; }
        public List<AllFeaturedResponse<int>> FeaturedResources { get; set; }
        public List<OptimisedSeriesRequest> FeaturedSeries { get; set; }
        public List<AllFeaturedResponse<int>> FeaturedTopics { get; set; }
        public List<OptimisedVideosResponse> FeaturedVideos { get; set; }
    }
}
