using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestStories.API.Filters;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;
using TestStories.API.Services.Errors;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api/userMedia")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(CustomAuthorizationFilter))]
    public class UserMediaController : ControllerBase
    {
        private readonly IUserMediaReadService _userMediaReadService;
        private readonly IUserMediaWriteService _userMediaWriteService;
        private readonly IUserReadService _userReadService;
        private int _userId = 0;
        private readonly ILogger<UserMediaController> _logger;

        /// <inheritdoc />
        public UserMediaController(IUserMediaReadService userMediaReadService, IUserReadService userReadService, 
            IUserMediaWriteService userMediaWriteService, ILogger<UserMediaController> logger)
        {
            _userMediaReadService = userMediaReadService;
            _userMediaWriteService = userMediaWriteService;
            _userReadService = userReadService;
            _logger = logger;
        }


        /// <summary>
        ///  Api to add User's playlist,  
        ///  Used At: End-User.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of newly added user's playlist </returns>
        [HttpPost("addPlayList")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlaylistModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PlaylistModel>> AddPlaylistAsync(AddPlaylistModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            model.UserId = _userId;
            return await _userMediaWriteService.AddPlaylistAsync(model);
        }


        /// <summary>
        /// Api to add Media to User's favorite,  
        /// Used At: End-User.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of newly added media to user's favourite.</returns>
        [HttpPost("addToFavourite")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FavouriteModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<FavouriteModel>> AddToFavouriteAsync(AddToFavoriteModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            model.UserId = _userId;
            return await _userMediaWriteService.AddToFavouriteAsync(model);
        }


        /// <summary>
        /// Api to remove Media from User's favorite,  
        /// Used At: End-User.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of recenlty deteted media from user's favourite.</returns>
        [HttpPost("removeFromFavourite")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FavouriteModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<FavouriteModel>> RemoveFromFavouriteAsync(AddToFavoriteModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            model.UserId = _userId;

            return await _userMediaWriteService.RemoveFromFavouriteAsync(model);
        }


        /// <summary>
        /// Api to add Media to playlist,  
        /// Used At: End-User.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of recently added media to playlist.</returns>
        [HttpPost("addToPlaylist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaPlayListModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<MediaPlayListModel>> AddToPlaylistAsync(AddToPlaylistModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _userMediaWriteService.AddToPlaylistAsync(model);
        }


        /// <summary>
        /// Api to edit User's playlist,  
        /// Used At: End-User.
        /// </summary>
        /// <param name="playlistId"></param>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of user's recently updated playlist.</returns>
        [HttpPut("playlists/{playlistId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlaylistModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PlaylistModel>> EditPlaylistAsync(int playlistId, EditPlayListModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _userMediaWriteService.EditPlaylistAsync(playlistId, model);
        }


        /// <summary>
        ///  Api to get Media playlist details, 
        ///  Used At: End-User.
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns>An object that contains the details of medias attached to particular playlist.</returns>
        [HttpGet("mediaPlaylist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<MediaInfoModel>))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CollectionModel<MediaInfoModel>>> MediaPlayListAsync(int playlistId)
        {
            return await _userMediaReadService.MediaPlayListAsync(playlistId);
        }


        /// <summary>
        /// Api to remove User's playlist,  
        /// Used At: End-User
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns>OK Response</returns>
        [HttpDelete("removePlaylist/{playlistId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemovePlaylistAsync(int playlistId)
        {
            await _userMediaWriteService.RemovePlaylistAsync(playlistId);
            return Ok();
        }


        /// <summary>
        ///  Api to get Media details, 
        ///  Used At: Admin and End-User.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns>An object that contains the details of media.</returns>
        [HttpGet("mediaDetails")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaViewModel))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<MediaViewModel>> MediaDetailAsync(int mediaId)
        {
            try
            {
                var currentRole = this.CurrentUserRole() ?? UserTypeEnum.User.ToString();
                return await _userMediaReadService.MediaDetailAsync(mediaId, currentRole);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiError(403, ex.Message));
            }
        }


        /// <summary>
        /// Api to subscribe series at user's subscription, 
        /// Used At: End-User
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of subscribed series at user's subscription</returns>
        [HttpPost("subscribeSeries")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubscribeSeriesModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SubscribeSeriesModel>> SubscribeSeriesAsync(SubscribeSeries model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            model.UserId = _userId;
            return await _userMediaWriteService.SubscribeSeriesAsync(model);
        }


        /// <summary>
        ///   Api to unsubscribe series from the user's subscription, 
        ///   Used At: End-User
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contais the details of unsubscribed series from user's subscription</returns>
        [HttpPost("unsubscribeSeries")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubscribeSeries))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SubscribeSeries>> UnsubscribeSeriesAsync(SubscribeSeries model)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            model.UserId = _userId;
            return await _userMediaWriteService.UnsubscribeSeriesAsync(model);
        }


        /// <summary>
        ///  Api to subscribe topic to user's subscription, 
        ///  Used At: End-User
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contais the details of subscribed topic at user's subscription</returns>
        [HttpPost("subscribeTopic")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubscribeTopicModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SubscribeTopicModel>> SubscribeTopicAsync(SubscribeTopic model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            model.UserId = _userId;
            return await _userMediaWriteService.SubscribeTopicAsync(model);
        }


        /// <summary>
        ///  Api to unsubscribe topic from user's subscription, 
        ///  Use At: End-User
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contais the details of unsubscribed topic from user's subscription</returns>
        [HttpPost("unsubscribeTopic")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubscribeTopic))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SubscribeTopic>> UnsubscribeTopicAsync(SubscribeTopic model)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            model.UserId = _userId;
            return await _userMediaWriteService.UnsubscribeTopicAsync(model);
        }


        /// <summary>
        ///  Api to get user's watch history, 
        ///  Used At: End-User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>An object that contains the colllection of user's watch history details</returns>
        [HttpGet("WatchHistory")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserWatchHistoryViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<UserWatchHistoryViewModel>>> WatchHistoryAsync(int userId)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            return await _userMediaReadService.WatchHistoryAsync(_userId);
        }


        /// <summary>
        ///  Api to add media to user's watch history, 
        ///  Used At: End-User
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns>An object that contains the details of  recently added media at user's watch history</returns>
        [HttpPost("watchHistory")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddWatchHistory))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<AddWatchHistory>> AddWatchHistoryAsync(long mediaId)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            return await _userMediaWriteService.AddWatchHistoryAsync(mediaId, _userId);
        }


        /// <summary>
        /// Api to remove media from user's watch history, 
        /// Used At: End-User
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns>An object that contains the details of recently removed media from the user's watch history.</returns>
        [HttpDelete("removeWatchHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemoveWatchHistoryAsync(long mediaId)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Delete: request for controller: UserMediaController and action: RemoveWatchHistoryAsync for mediaId:{mediaId} and userId- is: {_userId}");
           
            await _userMediaWriteService.RemoveWatchHistoryAsync(mediaId, _userId);
            return Ok();
        }
    }
}
