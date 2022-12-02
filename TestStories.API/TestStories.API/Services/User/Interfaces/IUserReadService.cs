using System.Collections.Generic;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public interface IUserReadService
    {
        Task<CollectionModel<ShortUserModel>> GetShortUsers (FilterUserRequest filter);

        Task<CollectionModel<UserAutoCompleteSearch>> UserAutoCompleteSearch ();

        Task<playListItem> GetUserPlayListMedia (int userId);

        Task<ShortUserModel> GetUserById(int id);

        List<Playlist> UserPlaylist (int userId);

        Task<List<UserSubscriptionModel>> GetUserSubscriptions (int userId);

        Task<List<FavouriteUserModel>> GetUserFavorites (int userId);

        Task<int> GetUserIdByEmail (string email);

        Task<(int? AdminUserTypeId, int? AdminEditorUserTypeId, int? PartnerUserTypeId)> GetAdminPartnerTypeIdsAsync ();

        Task<UserData> GetUserSubscriptionItems(int userId);

        Task<string> GetApiKeyByEmail(string email, System.Security.Claims.ClaimsPrincipal user);
        Task<bool> ValidateApiKey(string apiKey);
    }
}
