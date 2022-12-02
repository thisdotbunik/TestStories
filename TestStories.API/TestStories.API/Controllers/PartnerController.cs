using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestStories.API.Common;
using TestStories.API.Filters;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;

namespace TestStories.API.Controllers
{

    [Route("api/partners")]
    [ApiController]

    public class PartnerController : ControllerBase
    {
        private readonly IPartnerWriteService _partnerWriteService;
        private readonly IPartnerReadService _partnerReadService;
        private readonly IUserReadService _userReadService;
        private int _userId = 0;
        private readonly ILogger<PartnerController> _logger;
        public PartnerController(IPartnerWriteService partnerWriteService, IUserReadService userReadService, 
            IPartnerReadService partnerReadService, ILogger<PartnerController> logger)
        {
            _partnerWriteService = partnerWriteService;
            _partnerReadService = partnerReadService;
            _userReadService = userReadService;
            _logger = logger;
        }


        /// <summary>
        ///  Api to get Partner by parnerId, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the Partner Details.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Admin-Editor,Partner-User,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerDetailViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartnerDetailViewModel>> GetPartnerAsync(int id)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:GetPartnerAsync get by id {id} and userId {_userId}");

            var response = await _partnerReadService.GetPartnerAsync(id);
            if ( response != null )
            {
                return response;
            }

            return NotFound();
        }


        /// <summary>
        ///  Api to Unarchive Existing Partner, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OK Response</returns>
        [HttpPut("{id}/unarchive")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> UnarchivePartner(int id)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Update:Request received Controller:PartnerController and Action:UnarchivePartner get by id {id} and userId {_userId}");

            await _partnerWriteService.UnarchivePartner(id);
            return Ok();
        }


        /// <summary>
        /// Api to add new Partner, 
        /// Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the added Partner details.</returns>
        [HttpPost("addPartner")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerResponseModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartnerResponseModel>> AddPartner([FromForm]AddPartnerViewModel entity)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Add:Request received Controller:PartnerController and Action:AddPartner request is  {LogsandException.GetCurrentInputJsonString(entity)} and userId {_userId}");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return await _partnerWriteService.AddPartnerAsync(entity);
        }


        /// <summary>
        /// Api to edit Partner,  
        /// Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the updated partner details </returns>
        [HttpPut("editPartner/{partnerId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerResponseModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartnerResponseModel>> EditPartner(int partnerId, [FromForm]EditPartnerViewModel model)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Request received Controller:PartnerController and Action:EditPartner and input request is:  {LogsandException.GetCurrentInputJsonString(model)} and userId: {_userId}");
            
            return await _partnerWriteService.EditPartnerAsync(partnerId, model);
        }


        /// <summary>
        ///  Api to get filtered Partners, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of filtered partners.</returns>
        [HttpPost("all")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerDetailViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<PartnerDetailViewModel>>> SearchPartner(FilterPartnerViewRequest partnerFilter)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:SearchPartner and input request is:  {LogsandException.GetCurrentInputJsonString(partnerFilter)} and userId: {_userId}");

            return await _partnerReadService.SearchPartner(partnerFilter);
        }


        /// <summary>
        /// Api to get collection of Partners for dropdown, 
        /// Used At: Admin.
        /// </summary>
        /// <returns>An ojject that contains the colection of short details of partners.</returns>
        [HttpGet("searchPartnersAutocomplete")]
        [Authorize(Roles = "Admin,Admin-Editor,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerAutoCompleteSerachViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<PartnerAutoCompleteSerachViewModel>>> SearchPartnersAutoComplete()
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:SearchPartnersAutoComplete and userId: {_userId}");

            return await _partnerReadService.SearchPartnersAutoComplete();
        }


        /// <summary>
        /// Api to get partners, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns>An object that contains the collection of partners.</returns>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerDetailViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<PartnerDetailViewModel>>> getPartnerDetails(int? id, int page, int pageSize)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:getPartnerDetails and request id is:{id} and userId: {_userId}");

            return await _partnerReadService.GetPartnerDetails(id , page , pageSize);
        }


        /// <summary>
        ///  Api to get showcase Partners, 
        ///  Used At:Admin.
        /// </summary>
        /// <returns>An object that contains the collection of showcase partners.</returns>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<PartnerModel>>> getShowcasePartners()
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:getShowcasePartners and userId: {_userId}");

            return await _partnerReadService.GetShowcasePartners();
        }

        /// <summary>
        ///  Api to get Active Partners, 
        ///  Used At:Admin.
        /// </summary>
        /// <returns>An object that contains the collection of Active partners.</returns>
        [HttpGet("activePartners")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerModel))]
        public async Task<ActionResult<CollectionModel<PartnerModel>>> GetActivePartnersAsync()
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:GetActivePartnersAsync and userId: {_userId}");

            return await _partnerReadService.GetActivePartnersAsync();
        }


        /// <summary>
        ///   Api to validate Partner distribution Media,
        ///   Used At: Admin.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="mediaId"></param>
        /// <returns>OK Response</returns>
        [HttpGet("validatePartnerDistribution")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetDistributionMedia(long mediaId, int partnerId)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:GetDistributionMedia mediaid:{mediaId} and partnerId:{partnerId} and userId: {_userId}");

            var partnerMedia = await _partnerReadService.GetDistributionMedia(mediaId , partnerId);
            if (partnerMedia != null)
            {
                if (partnerMedia.StartDateUtc <= DateTime.UtcNow && DateTime.UtcNow <= partnerMedia.EndDateUtc)
                {
                    return Ok();
                }
            }
            return NotFound();
        }


        /// <summary>
        /// Api to get Content-Partner Medias, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="PageSize"></param>
        /// <param name="Page"></param>
        /// <returns>An object that contains the details of content-partner with medias count.</returns>
        [HttpGet("{id}/media")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartnerViewModel>> GetPartnerMedia(int id, int PageSize, int Page)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:GetPartnerMedia id:{id}  and userId: {_userId}");

            return await _partnerReadService.GetPartnerMedia(id , PageSize , Page);
        }


        /// <summary>
        /// Api to get details of Partners Distribution, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="_filter"></param>
        /// <returns>An object that contains the partners distribution media.</returns>
        [HttpPost("distributions")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerDistributionViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartnerDistributionDetailsViewModel>> GetPartnerDestribution(FilterPartnerDistributionViewRequest filter)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:GetPartnerDestribution input filter:{LogsandException.GetCurrentInputJsonString(filter)}  and userId: {_userId}");

            return await _partnerReadService.GetPartnerDistribution(filter);
        }


        /// <summary>
        ///  Api to get Distribution-Media for dropdown, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of distribution media.</returns>
        [HttpGet("distributionMediaAutoCompleteSearch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DistributionAutocompleteViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<DistributionAutocompleteViewModel>>> DistributionMediaAutoCompleteSearch()
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:DistributionMediaAutoCompleteSearch  and userId: {_userId}");

            return await _partnerReadService.DistributionMediaAutoCompleteSearch();
        }


        /// <summary>
        /// Api to get filtered distribution media, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="_filter"></param>
        /// <returns>An object that contains the filtered distribution media.</returns>
        [HttpPost("distributionMediaSearch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PartnerDistributionViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartnerDistributionDetailsViewModel>> DistributionMediaSearch(FilterPartnerDistributionViewRequest _filter)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:DistributionMediaSearch and input filter is {LogsandException.GetCurrentInputJsonString(_filter)}  and userId: {_userId}");

            return await _partnerReadService.DistributionMediaSearch(_filter);
        }


        /// <summary>
        ///  Api to delete the Partner, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OK Response</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemovePartnersAsync(int id)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Delete:Request received Controller:PartnerController and Action:RemovePartnersAsync id :{id} and userId: {_userId}");

            await _partnerWriteService.RemovePartnerAsync(id);
            
            return Ok();
        }


        /// <summary>
        ///  Api to rrchive existing Partner, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OK Response</returns>
        [HttpPut("{id}/archive")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ArchivePartnerAsync(int id)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Update:Request received Controller:PartnerController and Action:ArchivePartner for id :{id} and userId: {_userId}");

            await _partnerWriteService.ArchivePartnerAsync(id);
            return Ok();
        }


        /// <summary>
        ///  Api to expire Partner distribution Medias, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>OK response</returns>
        [HttpPut("distributions/expire")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ExpirePartnerDistribution(List<int> ids)
        {
            await _partnerWriteService.ExipreDistributionPartner(ids);
            return Ok();
        }


        /// <summary>
        ///  Api to update enddate of Partner distribution, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="endDate"></param>
        /// <returns>OK response</returns>
        [HttpPut("distributions/{id}/endDate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> UpdateEndDateOfPartnerDistribution(int id, string endDate)
        {
            await _partnerWriteService.updateEndDateOfDistributionPartner(id, Convert.ToDateTime(endDate));
            return Ok();
        }


        /// <summary>
        /// Api to get collection of Partner Medias, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An object that contains the collection of partner medias.</returns>
        [HttpPost("media")]
        [Authorize(Roles = "Partner-User,Admin-Editor,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<PartnerMediaViewModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<PartnerMediaViewModel>>> GetPartnerMedia(PartnerMediaFilterRequest request)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get:Request received Controller:PartnerController and Action:GetPartnerMedia for filter is:{LogsandException.GetCurrentInputJsonString(request)} and userId: {_userId}");

            return await _partnerReadService.GetPartnerMedia(request);
        }


        /// <summary>
        /// Api to get Partner Media for dropdown, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="filterString"></param>
        /// <returns>An object that contains the collection of filtered partner medias.</returns>
        [HttpPost("media/autoComplete")]
        [Authorize(Roles = "Partner-User,Admin-Editor,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<MediaAutoCompleteModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<MediaAutoCompleteModel>>> GetPartnerMediaAutoComplete(int partnerId, string filterString)
        {
            return await _partnerReadService.GetPartnerMediaAutoComplete(partnerId , filterString);
        }
    }
}