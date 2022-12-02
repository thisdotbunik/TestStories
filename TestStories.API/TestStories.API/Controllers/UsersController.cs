using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestStories.API.Filters;
using TestStories.API.Infrastructure.Errors;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;
using TestStories.Common;
using TestStories.Common.Configurations;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserWriteService _userWriteService;
        private readonly IUserReadService _userReadService;
        private int _userId = 0;
        private readonly ILogger<UsersController> _logger;
        readonly AppSettings _appSettings;

        /// <inheritdoc />
        public UsersController(IUserWriteService userWriteService, IUserReadService userReadService, IOptions<AppSettings> appSettings, ILogger<UsersController> logger)
        {
            _userWriteService = userWriteService;
            _userReadService = userReadService;
            _appSettings = appSettings.Value;
            _logger = logger;
        }


        /// <summary>
        /// Api to get collection of filtered users, 
        /// Used At: Admin and End-User
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>An object that conatains the collection of flitered users</returns>
        [HttpPost("shortInfo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ShortUserModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ShortUserModel>>> GetShortUsersAsync(FilterUserRequest filter)
        {
            return await _userReadService.GetShortUsers(filter);
        }


        /// <summary>
        /// Api to add a new user, 
        /// Used At: Admin
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>An object that contains the details of newly added user</returns>
        [HttpPost("addUser")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ShortUserModel>> AddUserAsync(AddUserModel entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userWriteService.CreateUserAsync(entity, User);
            return Ok(result);
        }

        /// <summary>
        /// Api to get the collection of users for dropdown, 
        /// Used At: Admin
        /// </summary>
        /// <returns>An object that contains the collection of users</returns>
        [HttpPost("UserAutoCompleteSearch")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<UserAutoCompleteSearch>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<UserAutoCompleteSearch>>> UserAutoCompleteSearch()
        {
            return await _userReadService.UserAutoCompleteSearch();
        }


        /// <summary>
        /// Api to update user, 
        /// Used At: Admin and End-User
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>An object that contains the details of recently updated user</returns>
        [HttpPost("editUser")]
        [Authorize]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ShortUserModel>> EditUserAsync(EditUserModel entity)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userWriteService.EditUserAsync(entity, User);
            return Ok(result);
        }


        /// <summary>
        /// Api to update user status, 
        /// Used At; Admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns>OK Response</returns>
        [HttpPost("changeUserStatus")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ChangeUserStatus(ChangeUserStatusModel model)
        {
            await _userWriteService.ChangeUserStatusAsync(model);
            return Ok();
        }


        /// <summary>
        /// Api to get short user info, 
        /// Used At: Admin and End-User
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the details of user</returns>
        [HttpGet("{id}")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShortUserModel>> Get(int id)
        {
            var roles = this.CurrentUserRole() ?? UserTypeEnum.User.ToString();
            if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            {
                var userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
                if (userId != id)
                {
                    return Unauthorized();
                }

            }
            var result = await _userReadService.GetUserById(id);
            if (result == null)
            {
                return NotFound();
            }

            return result;
        }


        /// <summary>
        /// Api to get user's playlist, 
        /// Used At: End-user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>An object that contains the user's paylists details</returns>
        [Authorize]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpGet("playlists")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(playListItem))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<playListItem>> Playlists(int userId)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Received: request for controller: UsersController and action: Playlists for userId {userId}  userId- is: {_userId}");
            return await _userReadService.GetUserPlayListMedia(_userId);
        }


        /// <summary>
        /// Api to get user's subscriptions, 
        /// Used At: End-User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>An object that contains the details of user's subscriptions.</returns>
        [Authorize]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpGet("subscriptions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserSubscriptionModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<UserSubscriptionModel>>> GetUserSubscriptions(int userId)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Received: request for controller: UsersController and action: GetUserSubscriptions and userId- is: {_userId}");

            var result = await _userReadService.GetUserSubscriptions(_userId);
            if (result != null)
            {
                return new CollectionModel<UserSubscriptionModel>
                {
                    Items = result,
                    TotalCount = result.Count
                };
            }

            return new CollectionModel<UserSubscriptionModel>();
        }


        /// <summary>
        /// Api to get user's favorites, 
        /// Used At: End-User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>An object that contains the details of user's favourites</returns>
        [Authorize]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpGet("favorites")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FavouriteUserModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<FavouriteUserModel>>> Favorites(int userId)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Received: request for controller: UsersController and action: Favorites and userId- is: {_userId}");

            var result = await _userReadService.GetUserFavorites(_userId);
            if (result != null)
            {
                return new CollectionModel<FavouriteUserModel>
                {
                    Items = result,
                    TotalCount = result.Count
                };
            }

            return new CollectionModel<FavouriteUserModel>();
        }


        /// <summary>
        /// Api to remove the User entity, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>OK Response</returns>
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OK Response</returns>
        [HttpDelete]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemoveUserAsync(string email, bool onlyClean = false)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Delete:Received: request for controller: UsersController and action: RemoveUserAsync with email is: {email} and userId- is: {_userId}");
           
            if (EnvironmentVariables.Env != EnvronmentTypeEnum.stage.ToString())
            {
                return UnprocessableEntity(new BusinessValidationError("You are not permitted to perform this operation"));
            }

            var whitelistAdmins = _appSettings.WhitelistAdmins.Split(',').ToList();
            if (whitelistAdmins.Contains(email))
            {
                return StatusCode(403, $"You are not permitted to delete {email}");
            }
           
            var isDeleted = await _userWriteService.RemoveUserAsync(email, onlyClean);
            if (!isDeleted)
            {
                return UnprocessableEntity(new BusinessValidationError("Can not remove the User. Please, try again."));
            }
            return Ok();
        }


        /// <summary>
        ///  Api to import db users at FusionAuth Server
        /// </summary>
        [HttpPost("importUsersAtFusionAuth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        public async Task<ActionResult<BaseResponse>> ImportUsersAtFusionAuth ()
        {
            await _userWriteService.ImportUsersAtFusionAuth();
            return Ok();
        }

        /// <summary>
        ///  Api to get user's playlists, subscriptions, favorites and watch history 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the user specific data</returns>
        [Authorize]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpGet("userSubscriptions")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(Task<UserData>))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<UserData> GetAllUserSubscriptions()
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Received: request for controller: UsersController and action: GetUserSpecificData and userId- is: {_userId}");
           
            return await _userReadService.GetUserSubscriptionItems(_userId);
        }

        /// <summary>
        /// Api to register Active(ready for login) User at application as well as FusionAuth, 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>OK Response</returns>
        [HttpPost("registerActiveUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RegisterActiveUser (RegisterActiveUser model)
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest(ModelState);
            }

            var isSuccess = await _userWriteService.RegisterActiveUser(model);
            if(isSuccess)
            {
                return Ok();
            }
            return UnprocessableEntity(new BusinessValidationError("Something went wrong, check the logs for more details"));
        }


        /// <summary>
        /// Api to get user info by email from db as well as fusion auth
        /// </summary>
        /// <param name="email"></param>
        /// <returns>An object that contains the details of user</returns>
        [HttpGet("getUserByEmail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseModel>> GetUserByEmail(string email)
        {
            var result = await _userWriteService.GetUserByEmail(email);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("getApiKey")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<ActionResult<string>> GetApiKey()
        {
            var result = await _userReadService.GetApiKeyByEmail(this.CurrentUserEmail(), User);
            return Ok(result);
        }

        [HttpPost("updateApiKey")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateApiKey([FromQuery]UpdateApiKeyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _userWriteService.UpdateApiKeyByEmail(this.CurrentUserEmail(), request.ApiKey, User);
            return Ok();
        }
    }
}
