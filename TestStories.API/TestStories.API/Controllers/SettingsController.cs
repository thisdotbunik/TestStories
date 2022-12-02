using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Filters;
using TestStories.API.Infrastructure.Errors;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services.Settings.Interfaces;
using TestStories.Common.Models.Events;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api/settings")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ServiceFilter(typeof(CustomAuthorizationFilter))]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingWriteService _settingWriteService;
        private readonly ISettingReadService _settingReadService;
        private readonly IPublishBlog _event;
        /// <inheritdoc />
        public SettingsController (ISettingWriteService settingWriteService , ISettingReadService settingReadService , IPublishBlog eEvent)
        {
            _settingWriteService = settingWriteService;
            _settingReadService = settingReadService;
            _event = eEvent;
        }


        /// <summary>
        ///  Api to get featured Carousel setting, 
        ///  Used At :Admin.
        /// </summary>
        /// <returns>An object that contains the details of featured carousel setting.</returns>
        [HttpGet("featuredCarousel")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(FeaturedCarouselSettingsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FeaturedCarouselSettingsModel>> GetFeaturedCarouselAsync ()
        {
            var result = await _settingReadService.GetFeaturedCarouselAsync();
            if ( result != null )
            {
                return result;
            }
            return NotFound();
        }


        /// <summary>
        ///  Api to get featured Topics setting, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the details of featured topics setting</returns>
        [HttpGet("featuredTopics")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(FeaturedTopicsSettingsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FeaturedTopicsSettingsModel>> GetFeaturedTopicsAsync ()
        {
            var result = await _settingReadService.GetFeaturedTopicsAsync();
            if ( result != null )
            {
                return result;
            }
            return NotFound();
        }


        /// <summary>
        ///  Api to get featured Series setting, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the details of features Series setting</returns>
        [HttpGet("featuredSeries")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(FeaturedSeriesSettingsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FeaturedSeriesSettingsModel>> GetFeaturedSeriesAsync ()
        {
            var result = await _settingReadService.GetFeaturedSeriesAsync();
            if ( result != null )
            {
                return result;
            }
            return NotFound();
        }


        /// <summary>
        ///  Api to get featured Resource setting, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the details of features Resource setting</returns>
        [HttpGet("featuredResources")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(FeaturedSeriesSettingsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FeaturedSeriesSettingsModel>> GetFeaturedResourcesAsync ()
        {
            var result = await _settingReadService.GetFeaturedResourcesAsync();
            if ( result != null )
            {
                return result;
            }
            return NotFound();
        }


        /// <summary>
        ///  Api to save featured Carousel setting, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of recently saved featured carousel setting.</returns>
        [HttpPost("featuredCarousel")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(FeaturedCarouselSettingsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<FeaturedCarouselSettingsModel>> SaveFeaturedCarouselAsync (
            SaveFeaturedCarouselSettingsModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            var result = await _settingWriteService.SaveFeaturedCarouselAsync(model);
            if ( result != null )
            {
                return result;
            }
            return UnprocessableEntity(new BusinessValidationError("Can not update settings. Please, try again."));
        }


        /// <summary>
        ///  Api to save featured Topics setting, 
        ///  Used At: Admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of recently saved featured topics setting</returns>
        [HttpPost("featuredTopics")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(FeaturedTopicsSettingsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<FeaturedTopicsSettingsModel>> SaveFeaturedTopicsAsync (
            SaveFeaturedTopicsSettingsModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            var result = await _settingWriteService.SaveFeaturedTopicsAsync(model);
            if(result != null)
            {
                return result;
            }
            return UnprocessableEntity(new BusinessValidationError("Can not update settings. Please, try again."));
        }


        /// <summary>
        ///  Api to save featured series setting, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of recently saved featured series setting.</returns>
        [HttpPost("featuredSeries")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(FeaturedSeriesSettingsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<FeaturedSeriesSettingsModel>> SaveFeaturedSeriesAsync (SaveFeaturedSeriesSettingsModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            var result = await _settingWriteService.SaveFeaturedSeriesAsync(model);
            if(result != null)
            {
                return result;
            }
            return UnprocessableEntity(new BusinessValidationError("Can not update settings. Please, try again."));
        }


        /// <summary>
        ///  Api to save featured Resources setting, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of recently saved featured Resources setting.</returns>
        [HttpPost("featuredResources")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(FeaturedSeriesSettingsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<FeaturedSeriesSettingsModel>> SaveFeaturedResourcesAsync (SaveFeaturedSeriesSettingsModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            var result = await _settingWriteService.SaveFeaturedResourcesAsync(model);
            if(result != null)
            {
                return result;
            }
            return UnprocessableEntity(new BusinessValidationError("Can not update settings. Please, try again."));
        }


        /// <summary>
        ///  Api to change showcased partner order, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>OK</returns>
        [HttpPost("apply-partners-sorting-order")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ChangeShowcasedPartnerOrderAsync (PartnerOrderModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            await _settingWriteService.ChangePartnerOrder(model);

            return Ok();
        }


        /// <summary>
        ///  Api to reset showcased partner order, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>OK</returns>
        [HttpPost("resetPartnerOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ResetPartnerOrder ()
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            await _settingWriteService.ResetPartnerOrder();

            return Ok();
        }


        /// <summary>
        ///  Api to Resync Blogs from Wordress, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>OK</returns>
        [HttpPost("resyncBlogs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ResyncBlogs ()
        {
            await _event.Publish();

            return Ok();
        }
    }
}