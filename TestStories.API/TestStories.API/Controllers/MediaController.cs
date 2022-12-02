using System;
using System.Collections.Generic;
using System.Linq;
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
using TestStories.Common;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Controllers
{
    [Route("api/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaReadService _mediaReadService;
        private readonly IMediaWriteService _mediaWriteService;
        private readonly IUserReadService _userReadService;
        private readonly ILogger<MediaController> _logger;

        private int _userId = 0;

       /// <summary>
       /// Constructor
       /// </summary>
       /// <param name="repo"></param>
       /// <param name="mediaReadService"></param>
       /// <param name="logger"></param>
        public MediaController(IMediaWriteService mediaWriteService, IMediaReadService mediaReadService, IUserReadService userReadService ,  ILogger<MediaController> logger)
        {
            _mediaWriteService = mediaWriteService;
            _mediaReadService = mediaReadService;
            _userReadService = userReadService;
            _logger = logger;
        }


        /// <summary>
        ///  Api to get Medias based on filter criteria, 
        ///  Used At :Admin.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An object that contains the collection of filtered media.</returns>
        ///        
        [HttpPost("MediaSearch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MediaViewModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GridResponse<MediaListResponse>>> MediaSearch(FilterMediaRequest request)
        {
            return await _mediaReadService.MediaSearch(request);
        }

        /// <summary>
        /// Retrieves Media entity by id. 
        /// Used At: Admin and End-User.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the details of media entity</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaViewModel))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<MediaViewModel>> GetMediaAsync(int id)
        {
            try
            {
                var currentRole = this.CurrentUserRole() ?? UserTypeEnum.User.ToString();
                return await _mediaReadService.GetMediaAsync(id, currentRole);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiError(403, ex.Message));
            }
        }

        /// <summary>
        /// Retrieves Media details by id. 
        /// Used At: End-User.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the details of media entity</returns>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaViewModel))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<MediaViewModel>> GetMediaByIdAsync(int id)
        {
            try
            {
                return await _mediaReadService.GetMediaByIdAsync(id);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiError(403, ex.Message));
            }
        }


        /// <summary>
        ///  Api to get collection of Medias for Dropdown, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of media entities</returns>
        /// 
        [HttpGet("MediaAutoCompleteSearch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaAutoCompleteModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GridResponse<MediaAutoCompleteModel>>> MediaAutoCompleteSearch()
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Search:fulfilled request for controller: MediaControllerand action MediaAutoCompleteSearch By Name and userId {_userId}");

            return await _mediaReadService.MediaAutoCompleteSearch();

        }


        /// <summary>
        ///   Api to get short details of Media entities, 
        ///   Used At: Admin and End-User.
        /// </summary>
        /// <returns>An object that contains the collection of short media entities</returns>
        [HttpGet("shortInfo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICollection<MediaShortModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<MediaShortModel>>> GetMediaShortInfoAsync()
        {
            var result = await _mediaReadService.GetMediaShortInfoAsync();
            if (result != null)
            {
                return result;        
            }
            return NotFound();
        }


        /// <summary>
        ///   Api to get Media Carousel info, 
        ///   Used At: End-User.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the collection of Carousel Media</returns>
        [HttpPost("carouselInfo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICollection<MediaInfoModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<MediaInfoModel>>> GetMediaCarouselInfoAsync(IdsRequest<long> model)
        {
            if (model?.Ids == null || !model.Ids.Any())
            {
                return NotFound();
            }

            var result = await _mediaReadService.GetMediaCarouselInfoAsync(model);
            if (result != null)
            {
                return result;
            }
            return NotFound();
        }


        /// <summary>
        ///  Api to Archive media based on mediaId, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns>An object that contains the Base Response.</returns>
        [HttpPost("ArchiveMedia")]
        [Authorize(Roles = "Admin,Admin-Editor,Partner-User,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse>> ArchiveMedia(int mediaId)
        {
             _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            var response = await _mediaWriteService.ArchiveMediaAsync(mediaId, _userId, this.CurrentUserRole());
            if (response.ErrorCode == 404) return NotFound();
            return response;
        }


        /// <summary>
        ///    Api to UnArchive Media based on MediaId, 
        ///    Used At: Admin.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns>An object that contains the Base Response.</returns>
        [HttpPost("unarchiveMedia")]
        [Authorize(Roles = "Admin,Admin-Editor,Partner-User,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse>> UnarchiveMedia(int mediaId)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            var response = await _mediaWriteService.UnarchiveMediaAsync(mediaId, _userId, this.CurrentUserRole());
             if (response.ErrorCode == 404) return NotFound();
             return response;
        }


        /// <summary>
        /// Api to add a Media entity, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the newly added media detail</returns>
        [HttpPost("addMedia")]
        [Authorize(Roles = "Admin,Admin-Editor,Partner-User,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaShortModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<MediaShortModel>> AddMediaAsync([FromForm] AddMediaModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            return await _mediaWriteService.AddMediaAsync(model, _userId);
        }


        /// <summary>
        /// Api to edit the Media entity, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the updated media details.</returns>
        [HttpPost("editMedia")]
        [Authorize(Roles = "Admin,Admin-Editor,Partner-User,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaShortModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<MediaShortModel>> EditMediaAsync([FromForm] EditMediaModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            return await _mediaWriteService.EditMediaAsync(model, _userId);
        }


        /// <summary>
        ///   Api to update all Medias at Cloud, 
        ///   Used At: Admin.
        /// </summary>
        /// <returns> Ok Response</returns>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpPost("UpdateAllMediaOnCloud")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateAllMediaOnCloud()
        {
            await _mediaWriteService.UpdateAllMediaOnCloud();
            return Ok();
        }


        /// <summary>
        /// Api to attach Media to specific Partner, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of attached media to partner. </returns>
        [HttpPost("sendToPartner")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerMediaModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PartnerMediaModel>> SendToPartnerAsync(AddSendToPartnerModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return await _mediaWriteService.SendToPartnerAsync(model);
        }


        /// <summary>
        ///    Api to get Media playlist by PlaylistId, 
        ///    Used At: End-User.
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns>An object that contains the collection of playlist medias</returns>
        [HttpGet("mediaPlaylist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<MediaInfoModel>))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CollectionModel<MediaInfoModel>>> MediaPlayList(int playlistId)
        {
            return await _mediaReadService.MediaPlayListAsync(playlistId);
        }


        /// <summary>
        ///  Api to get filtered media by media type and status, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of filtered media.</returns>
        [HttpPost("filteredMedia")]
        [Authorize]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaShortModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GridResponse<MediaShortModel>>> FilteredMedia(FilterMediaSearchRequest model)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"fulfilled request for controller: MediaController and action FilteredMedia By Name and userId {_userId}");
            return await _mediaReadService.FilteredMedia(model);
        }

        /// <summary>
        ///   Api to generate HLS URL, 
        ///   Used At: Admin.
        /// </summary>
        /// <returns> Ok Response</returns>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpPost("generateHlsUrl")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GenerateHlsUrl()
        {
            await _mediaWriteService.GenerateHlsUrl();
            return Ok();
        }

        /// <summary>
        /// Removes Media entity by title. 
        /// Used At: Admin.
        /// </summary>
        /// <param name="title"></param>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("remove-by-title")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteMediaByTypeAsync(string title)
        {
            if (EnvironmentVariables.Env != "dev" && EnvironmentVariables.Env != "stage")
            {
                throw new NotSupportedException();
            }

            await _mediaWriteService.DeleteMediaByTitle(title);
            return Ok();
        }


        /// <summary>
        ///   Api to get generated New Seourl, 
        ///   Used At: Admin.
        /// </summary>
        /// <returns> An object that contains the collection of media SeoUrl details</returns>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpGet("generateNewSeoUrl")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<MediaSeoDetailModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<MediaSeoDetailModel>>> GenerateNewSeoUrl()
        {
            var result = await _mediaWriteService.GenerateNewSeoUrl();
            return new CollectionModel<MediaSeoDetailModel>
            { Items = result, TotalCount = result.Count };
        }

        /// <summary>
        ///   Api to get updated Seourl, 
        ///   Used At: Admin.
        /// </summary>
        /// <returns>  An object that contains the collection of updated media SeoUrl details</returns>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpGet("getUpdatedSeoUrl")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<MediaSeoDetailModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<MediaSeoDetailModel>>> GetUpdatedSeoUrl()
        {
            var result = await _mediaWriteService.GetUpdatedSeoUrl();
            return new CollectionModel<MediaSeoDetailModel>
            { Items = result, TotalCount = result.Count };
        }

        /// <summary>
        ///   Api to Update SeoUrls for media, 
        ///   Used At: Admin.
        /// </summary>
        /// <returns> Ok Response</returns>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpPost("updateMediaSeoUrl")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateMediaSeoUrl()
        {
            await _mediaWriteService.UpdateMediaSeoUrl();
            return Ok();
        }

        /// <summary>
        /// Api to soft delete the Media entity, 
        /// Used At: SuperAdmin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OK Response</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemoveMediaAsync(long id)
        {
            await _mediaWriteService.DeleteMediaById(id);
            return Ok();
        }

        /// <summary>
        ///  Api to update media uniqueIds, 
        ///  Used At: Admin, SuperAdmin. 
        /// </summary>
        /// <returns> OK Response</returns>
        [HttpPost("updateMediaUniqueIds")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> UpdateMediaUniqueIds(string fileName)
        {
            _logger.LogDebug($"Update:Request received Controller:MediaController and Action:UpdateMediaUniqueIds");

            await _mediaWriteService.UpdateMediaUniqueIds(fileName);
            return Ok();
        }
    }
}
