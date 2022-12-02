using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IUserMediaWriteService
    {
        Task<PlaylistModel> AddPlaylistAsync (AddPlaylistModel model);
        Task RemovePlaylistAsync (int playlistId);
        Task<PlaylistModel> EditPlaylistAsync (int playlistId , EditPlayListModel model);
        Task<FavouriteModel> AddToFavouriteAsync (AddToFavoriteModel model);
        Task<FavouriteModel> RemoveFromFavouriteAsync (AddToFavoriteModel model);
        Task<MediaPlayListModel> AddToPlaylistAsync (AddToPlaylistModel model);       
        Task<SubscribeSeriesModel> SubscribeSeriesAsync (SubscribeSeries model);
        Task<SubscribeSeries> UnsubscribeSeriesAsync (SubscribeSeries model);
        Task<SubscribeTopicModel> SubscribeTopicAsync (SubscribeTopic model);
        Task<SubscribeTopic> UnsubscribeTopicAsync (SubscribeTopic model);
        Task<AddWatchHistory> AddWatchHistoryAsync (long mediaId , int userId);
        Task RemoveWatchHistoryAsync (long mediaId, int userId);
    }
}
