using System.Security.Claims;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public interface IUserWriteService
    {
        Task<ShortUserModel> CreateUserAsync(AddUserModel entity, ClaimsPrincipal claimsPrincipal);

        Task<ShortUserModel> EditUserAsync (EditUserModel entity , ClaimsPrincipal claimsPrincipal);

        Task ChangeUserStatusAsync(ChangeUserStatusModel entity);
        Task ChangeUserPasswordAsync(string newPassword);

        Task<bool> RemoveUserAsync(string email, bool onlyClean);

        Task<User> SubscribeNewsletter (int userId);

        Task<(int? AdminUserTypeId, int? AdminEditorUserTypeId, int? PartnerUserTypeId)> GetAdminPartnerTypeIdsAsync ();

        Task ImportUsersAtFusionAuth ();
        Task<bool> RegisterActiveUser (RegisterActiveUser model);
        Task<UserResponseModel> GetUserByEmail(string email);
        Task UpdateApiKeyByEmail(string email, string apiKey, ClaimsPrincipal user);
    }
}
