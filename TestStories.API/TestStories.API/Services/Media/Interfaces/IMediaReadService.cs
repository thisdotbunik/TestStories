using System.Collections.Generic;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IMediaReadService
    {
        Task<GridResponse<MediaListResponse>> MediaSearch(FilterMediaRequest request);
        Task<MediaViewModel> GetMediaAsync(int id, string userRole);
        Task<GridResponse<MediaAutoCompleteModel>> MediaAutoCompleteSearch();
        Task<List<MediaShortModel>> GetMediaShortInfoAsync();
        Task<List<MediaInfoModel>> GetMediaCarouselInfoAsync(IdsRequest<long> model);
        Task<GridResponse<MediaShortModel>> FilteredMedia(FilterMediaSearchRequest model);
        Task<CollectionModel<MediaInfoModel>> MediaPlayListAsync(int playlistId);
        Task<MediaViewModel> GetMediaByIdAsync(int id);
    }
}
