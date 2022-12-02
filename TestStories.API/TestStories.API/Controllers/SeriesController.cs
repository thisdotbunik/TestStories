using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Filters;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;
using TestStories.Common;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api/series")]
    [ApiController]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesWriteService _seriesWriteService;
        private readonly ISeriesReadService _seriesReadService;

        /// <inheritdoc />
        public SeriesController(ISeriesWriteService seriesWriteService , ISeriesReadService seriesReadService)
        {
            _seriesWriteService = seriesWriteService;
            _seriesReadService = seriesReadService;
        }


        /// <summary>
        ///   Api to get collection of filtered Series, 
        ///   Used At: Admin.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>An object that contains the collection of filtered Series.</returns>
        [HttpPost("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<SeriesViewModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<SeriesViewModel>>> GetSeriesViewCollectionAsync(SeriesViewRequest filter)
        {
            return await _seriesReadService.GetAllSeriesAsync(filter);
        }


        /// <summary>
        /// Api to get collection of Series for dropdown, 
        /// Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of series short details.</returns>
        [HttpGet("autoCompleteSearch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<SeriesAutoCompleteModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<SeriesAutoCompleteModel>>> SeriesAutoCompleteSearch()
        {
            return await _seriesReadService.SeriesAutoCompleteSearch();
        }


        /// <summary>
        ///  Api to get Series entity, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the series details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SeriesModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SeriesModel>> GetSeriesAsync(int id)
        {
            var response = await _seriesReadService.GetSeriesAsync(id);
            if(response != null)
            {
                return response;
            }
                
            return NotFound();
        }

        /// <summary>
        ///  Api to get Series short info, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of series short details.</returns>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("shortInfo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ShortSeriesModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ShortSeriesModel>>> GetShortSeriesAsync()
        {
            return await _seriesReadService.GetShortSeriesAsync();
        }


        /// <summary>
        ///  Api to add new Series, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of added Series.</returns>
        [HttpPost("addSeries")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SeriesModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SeriesModel>> AddSeriesAsync([FromForm] AddSeriesModel model)
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest(ModelState);
            }
            var result = await _seriesWriteService.AddSeriesAsync(model);
            return Ok(result);
        }


        /// <summary>
        ///  Api to edit the Series entity, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of updated Series.</returns>
        [HttpPost("editSeries")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SeriesModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SeriesModel>> EditSeriesAsync([FromForm] EditSeriesModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _seriesWriteService.EditSeriesAsync(model);
            return Ok(result);
        }


        /// <summary>
        /// Api to remove the Series entity, 
        /// Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OK Response</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemoveSeriesAsync(int id)
        {
            await _seriesWriteService.RemoveSeriesAsync(id);
           
            return Ok();
        }

        /// <summary>
        ///  Api to update All Series on Cloud, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>OK Response</returns>
        [HttpPost("updateCloudSeries")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCloudSeries()
        {
            await _seriesWriteService.UpdateCloudSeries();
            return Ok();
        }

        /// <summary>
        ///  Api to migrate all db Series on Cloud, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>OK Response</returns>
        [HttpPost("migrateDbSeriesToCloud")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> MigrateDbSeriesToCloud()
        {
            await _seriesWriteService.MigrateDbSeriesToCloud();
            return Ok();
        }


        /// <summary>
        /// Removes Series entity by name. 
        /// Used At: Admin.
        /// </summary>
        /// <param name="name"></param>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("remove-by-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteSeriesByNameAsync(string name)
        {
            if (EnvironmentVariables.Env != "dev" && EnvironmentVariables.Env != "stage")
            {
                throw new NotSupportedException();
            }

            await _seriesWriteService.DeleteSeriesByName(name);
            return Ok();
        }
    }
}