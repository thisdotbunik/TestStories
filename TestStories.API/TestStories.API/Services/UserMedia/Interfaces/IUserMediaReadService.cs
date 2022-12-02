using System.Threading.Tasks;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IUserMediaReadService
    {
        Task<MediaViewModel> MediaDetailAsync (int mediaId, string userRole);
        Task<CollectionModel<UserWatchHistoryViewModel>> WatchHistoryAsync (int userId);
        Task<CollectionModel<MediaInfoModel>> MediaPlayListAsync(int playlistId);
    }
}
