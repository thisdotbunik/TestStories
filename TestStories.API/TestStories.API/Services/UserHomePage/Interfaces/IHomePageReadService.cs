using System.Collections.Generic;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.CloudSearch.Service.Model;

namespace TestStories.API.Services
{
    public interface IHomePageReadService
    {
        Task<CollectionModel<FeaturedVideosModel>> GetFeaturedVideosAsync ();
        Task<CollectionModel<SuggestedSeriesMediaModel>> GetFeaturedSeriesAsync ();
        Task<CollectionModel<TopicInfoModel>> GetFeaturedTopicsAsync ();
        Task<CollectionModel<FeaturedResourceModel>> GetFeaturedResourcesAsync ();
        Task<CollectionModel<EditorPickModel>> GetEditorPicksAsync ();
        Task<UserFacingSearchViewModel> GetUserFacingCloudSearchAsync (string filterString , int pageSize , int pageNumber, bool isIncludeEmbedded);
        Task<SeriesMediaModel> SeriesMediaDetailsAsync (int seriesId , int pageNumber, int pageSize, bool isIncludeEmbedded);
        Task<TopicMediaModel> TopicMediaDetailsAsync (int topicId , int pageNumber, int pageSize, bool isIncludeEmbedded);
        Task<CollectionModel<MediaInfoModel>> UpcomingMediasAsync (long mediaId , int pageNumber , int pageSize , int itemType);
        Task<CollectionModel<FeaturedResourceModel>> FilteredResourcesAsync (FilterRequestModel model);
        Task<CollectionModel<FilteredSeriesResponse>> FilteredSeriesAsync (SeriesFilterRequest model);
        Task<List<FilteredResourceModel>> ResourcesStatisticsAsync ();
        Task<CollectionModel<ToolViewModel>> GetToolsByTopicIdAsync (int topicId , int pageNumber , int pageSize);
        Task<CollectionModel<BlogResponse>> GetFeaturedBlogsAsync ();
        
        Task<AllInOne> GetAll();
     
    }
}
