using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Services;
using System.Collections.Generic;
using System.Linq;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using System;
using TestStories.API.Services.Errors;
using TestStories.Common;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api/standalone")]
    [ApiController]
    public class StandaloneController :  ControllerBase
    {
        private readonly ISeriesStandaloneReadService _seriesReadService;
        private readonly IMediaStandaloneReadService _mediaReadService;
        private readonly IUserReadService _userReadService;

        public StandaloneController(ISeriesStandaloneReadService seriesReadService, IMediaStandaloneReadService mediaReadService, IUserReadService userReadService)
        {
            _seriesReadService = seriesReadService;
            _mediaReadService = mediaReadService;
            _userReadService = userReadService;
        }

        /// <summary>
        ///  Api to get Series entity, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the series details</returns>
        [HttpGet("series")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetSeriesStandaloneEndpoint([FromQuery]FilterSeriesStandaloneModel model)
        {
            bool isApiKeyValid = await _userReadService.ValidateApiKey(model.ApiKey);
            if (!isApiKeyValid)
            {
                return Forbid();
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _seriesReadService.GetSeriesStandaloneAsync(model.Fields, model.SeriesId);
            if (response != null && response.Any())
            {
                return Ok(response);
            }

            return NotFound();
        }

        /// <summary>
        /// Retrieves Media details by id. 
        /// Used At: End-User.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the details of media entity</returns>
        [HttpGet("media")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<dynamic>> GetMediaByIdAsync([FromQuery] FilterMediaStandaloneModel model)
        {
            bool isApiKeyValid = await _userReadService.ValidateApiKey(model.ApiKey);
            if (!isApiKeyValid)
            {
                return Forbid();
            }


            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _mediaReadService.GetMediaStandaloneAsync(model.MediaTypes, model.Ids, model.Fields);

            if (response != null && response.Any())
            {
                return Ok(response);
            }

            return NotFound();
        }

        /// <summary>
        ///  Api to get Media signed url by id, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the series details</returns>
        [HttpGet("mediaDownload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<string>> GetMediaDownload([FromQuery] GetDownloadMediaStandaloneModel model)
        {
            bool isApiKeyValid = await _userReadService.ValidateApiKey(model.ApiKey);
            if (!isApiKeyValid)
            {
                return Forbid();
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            string response = await _mediaReadService.GetMediaDownloadUrlStandaloneAsync(model.Id);
            if (response != null)
            {
                return Ok(response);
            }

            return NotFound();
        }

    }
}
