using System;
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
using TestStories.DataAccess.Enums;

namespace TestStories.API.Controllers
{
    [Route("api/experiment")]
    [ApiController]
    public class ExperimentController : ControllerBase
    {
        private readonly IExperimentWriteService _experimentWriteService;
        private readonly IExperimentReadService _experimentReadService;
        private readonly IUserReadService _userReadService;
        private readonly ILogger<ExperimentController> _logger;
        private int _userId = 0;
        /// <inheritdoc />
        public ExperimentController (IExperimentWriteService experimentWriteService , IExperimentReadService experimentReadService ,
            IUserReadService userReadService, ILogger<ExperimentController> logger)
        {

            _experimentReadService = experimentReadService;
            _experimentWriteService = experimentWriteService;
            _userReadService = userReadService;
            _logger = logger;
        }

        /// <summary>
        ///  Api to get Experiments with filter, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the filtered Experiements</returns>
        [HttpPost("all")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(ExperimentListModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CollectionModel<ExperimentListModel>>> FilterExperimentAsync (ExperimentFilterRequest experimentFilter)
        {
            return await _experimentReadService.FilterExperimentAsync(experimentFilter);
        }


        /// <summary>
        ///  Api to get Experiments for autocomplete dropdown, 
        ///  Used At: Admin
        /// </summary>
        /// <returns>An object that contains the all Experiments</returns>
        [HttpGet("autoComplete")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(ExperimentAutoComplete))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CollectionModel<ExperimentAutoComplete>>> ExperiementAutoCompleteAsync ()
        {
            return await _experimentReadService.ExperiementAutoCompleteAsync();
        }


        /// <summary>
        ///   Api to get Experiment entity, 
        ///   Used At: Admin.
        /// </summary>
        /// <param name="id">
        /// id of the Experiment.
        /// </param>
        /// <returns>Ab object that contains the Experiment details.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(ExperimentViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ExperimentViewModel>> GetExperiementAsync (int id)
        {
            var result = await _experimentReadService.GetExperiementAsync(id);
            if ( result != null )
            {
                return result;
            }
            return NotFound();
        }


        /// <summary>
        /// Api to add new Experiment, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the newly added Experiement.</returns>
        [HttpPost("addExperiment")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(ExperimentViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ExperimentViewModel>> AddExperimentAsync (AddExperimentModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            return await _experimentWriteService.AddExperimentAsync(model , _userId);
        }


        /// <summary>
        ///  Api to edit Experiment,  
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the updated experiement details</returns>
        [HttpPut("editExperiment/{experimentId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(ExperimentViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ExperimentViewModel>> EditExperimentAsync (int experimentId , EditExperimentModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return await _experimentWriteService.EditExperimentAsync(experimentId , model);
        }


        /// <summary>
        /// Api to update Experiment Status, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>OK Response</returns>
        [HttpPut("updateStatus")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ExperimentViewModel>> UpdateExperimentStatus (UpdateExperimentModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);
            return await _experimentWriteService.UpdateExperimentStatusAsync(model);
        }


        /// <summary>
        /// Api to track Experiment Event, 
        /// Used At: Admin and End-User. 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>OK Response</returns>
        [HttpPost("trackEvent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> TrackEvent (TrackEventRequest model)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Track :fulfilled request for controller: ExperimentController and action TrackEvent By evenTypeId and mediaId {_userId}");
            await _experimentWriteService.TrackEvent(model.EventTypeId , model.MediaId);
            return Ok();
        }
    }
}