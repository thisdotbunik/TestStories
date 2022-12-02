using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;
using TestStories.CloudSearch.Service.Model;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api/userHomepage")]
    [ApiController]
    public class UserHomepageController : ControllerBase
    {
        private readonly IHomePageReadService _homePageReadService;
        /// <inheritdoc />
        public UserHomepageController (IHomePageReadService homePageReadService)
        {
            _homePageReadService = homePageReadService;
        }

        /// <summary>
        ///  Api to get featured Videos, Blogs, Resources, Topics and Series 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the collection of featured medias.</returns>
        [HttpGet("featuredItems")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AllInOne))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<AllInOne> GetAllEndpointsAsync()
        {
            return await _homePageReadService.GetAll();
        }


        /// <summary>
        ///  Api to get featured Videos, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the collectin of featured medias.</returns>
        [HttpGet("featuredVideos")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<FeaturedVideosModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<FeaturedVideosModel>>> GetFeaturedVideosAsync ()
        {
            return await _homePageReadService.GetFeaturedVideosAsync();
        }


        /// <summary>
        ///  Api to get featured Series, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the collectin of featured series.</returns>
        [HttpGet("featuredSeries")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<SuggestedSeriesMediaModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<SuggestedSeriesMediaModel>>> GetFeaturedSeriesAsync ()
        {
            return await _homePageReadService.GetFeaturedSeriesAsync();
        }

     

        /// <summary>
        ///  Api to get featured Topics, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the collectin of featured topics.</returns>
        [HttpGet("featuredTopics")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<TopicInfoModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<TopicInfoModel>>> GetFeaturedTopicsAsync ()
        {
            return await _homePageReadService.GetFeaturedTopicsAsync();
        }

       
        /// <summary>
        ///  Api to get featured Resources, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the collection of featured Resources.</returns>
        [HttpGet("featuredResurces")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<FeaturedResourceModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<FeaturedResourceModel>>> GetFeaturedResourcesAsync ()
        {
            return await _homePageReadService.GetFeaturedResourcesAsync();
        }


        /// <summary>
        ///  Api to get Editor Picks, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the collectin of editor picks.</returns>
        [HttpGet("editorPicks")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<EditorPickModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<EditorPickModel>>> GetEditorPicksAsync ()
        {
            return await _homePageReadService.GetEditorPicksAsync();
        }


        /// <summary>
        ///  Api to get user facing cloud search, 
        ///  Used At: End-User.
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="isIncludeEmbedded"></param>
        /// <returns>An object that contains the searh result from the cloud</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(UserFacingSearchViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserFacingSearchViewModel>> GetUserFacingCloudSearchAsync (string filterString , int pageSize = 10 , int pageNumber = 1 , bool isIncludeEmbedded = true)
        {
            return await _homePageReadService.GetUserFacingCloudSearchAsync(filterString , pageSize , pageNumber, isIncludeEmbedded);
        }


        /// <summary>
        /// Api to get Series Medias details, 
        /// Used At: End-User.
        /// </summary>
        /// <param name="seriesId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="isIncludeEmbedded"></param>
        /// <returns>An object that contains the details of medias attached to particular series</returns>
        [HttpGet("seriesMediaDetails")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(SeriesMediaModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SeriesMediaModel>> SeriesMediaDetailsAsync (int seriesId , int pageNumber = 1, int pageSize = 12, bool isIncludeEmbedded = true)
        {
            var result = await _homePageReadService.SeriesMediaDetailsAsync(seriesId , pageNumber, pageSize, isIncludeEmbedded);

            if ( result != null )
            {
                return result;
            }
            return NotFound();
        }

        /// <summary>
        /// Api to get Topic Medias details, 
        /// Used At: End-User.
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="isIncludeEmbedded"></param>
        /// <returns>An object that contains the details of  media attached to particular topic</returns>
        [HttpGet("topicMediaDetails")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(SeriesMediaModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<TopicMediaModel>> TopicMediaDetailsAsync (int topicId , int pageNumber = 1, int pageSize = 12, bool isIncludeEmbedded = true)
        {
            var result = await _homePageReadService.TopicMediaDetailsAsync(topicId , pageNumber, pageSize, isIncludeEmbedded);

            if ( result != null )
            {
                return result;
            }
            return NotFound();
        }


        /// <summary>
        /// Api to get upcoming Medias, 
        /// Used At: End-User.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="itemType"></param>
        /// <returns>An object that contains the collection of medias that will be populated at See-What-Next section.</returns>
        [HttpGet("upcomingMedias")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<MediaInfoModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CollectionModel<MediaInfoModel>>> UpcomingMediasAsync (long mediaId , int pageNumber , int pageSize , int itemType = 0)
        {
            return await _homePageReadService.UpcomingMediasAsync(mediaId , pageNumber , pageSize , itemType);
        }

        /// <summary>
        ///  Api to get filtered resources from cloud, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the searh result from the cloud</returns>
        [HttpGet("filteredResources")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<FeaturedResourceModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<FeaturedResourceModel>>> FilteredResourcesAsync ([FromQuery] FilterRequestModel model)
        {
            return await _homePageReadService.FilteredResourcesAsync(model);
        }

        /// <summary>
        ///  Api to get filtered series. 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the filtered series</returns>
        [HttpGet("filteredSeries")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<FilteredSeriesResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<FilteredSeriesResponse>>> FilteredSeriesAsync ([FromQuery] SeriesFilterRequest model)
        {
            return await _homePageReadService.FilteredSeriesAsync(model);
        }

        /// <summary>
        ///  Api to get all resources with details from cloud, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the searh result from the cloud</returns>
        [HttpGet("resourcesStatistics")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<FilteredResourceModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<FilteredResourceModel>>> ResourcesStatisticsAsync ()
        {
            return await _homePageReadService.ResourcesStatisticsAsync();
        }
         
        ///  Api to get tool details by topicId, 
        ///  Used At: End-User
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the details of tools.</returns>
        [HttpGet("toolDetails")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<ToolViewModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ToolViewModel>>> GetToolsByTopicIdAsync (int topicId , int pageNumber = 1 , int pageSize = 10)
        {
            return await _homePageReadService.GetToolsByTopicIdAsync(topicId , pageNumber , pageSize);
        }


        /// <summary>
        ///  Api to get featured Blogs, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the collection of featured Blogs.</returns>
        [HttpGet("featuredBlogs")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CollectionModel<BlogResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<BlogResponse>>> GetFeaturedBlogsAsync ()
        {
            return await _homePageReadService.GetFeaturedBlogsAsync();
        }

      
    }
}
