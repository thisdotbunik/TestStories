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
    [Route("api/tools")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly IToolsWriteService _toolWriteService;
        private readonly IToolsReadService _toolReadService;

        /// <inheritdoc />
        public ToolsController(IToolsWriteService toolWriteService, IToolsReadService toolReadService)
        {
            _toolWriteService = toolWriteService;
            _toolReadService = toolReadService;
        }


        /// <summary>
        ///   Api to get filtered collection of Tools, 
        ///   Used At: Admin.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>An object that contains the collection of filtered tools.</returns>
        [HttpPost("all")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ShortToolModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ShortToolModel>>> GetToolViewCollectionAsync(FilterToolViewRequest filter)
        {
            return await _toolReadService.GetToolViewCollectionAsync(filter);
        }


        /// <summary>
        ///  Api to get collection of Tools for showMenu, 
        ///  Used At: End-User.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>An object that contains the collection of tools which will be show on Menu.</returns>
        [HttpPost("menu")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ShortToolModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ShortToolModel>>> GetClientTools(FilterClientToolViewRequest filter)
        {
            return await _toolReadService.GetClientTools(filter);
        }


        /// <summary>
        ///  Api to get collection of resources, 
        ///  Used At: End-User.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>An object that contains the collection of resources.</returns>
        [HttpGet("resources")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ToolItemOutput>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ToolItemOutput>>> GetResources(int page, int pageSize)
        {
            return await _toolReadService.GetResources(new FilterClientToolViewRequest() { Page = page, PageSize = pageSize});
        }


        /// <summary>
        /// Removes resource  by name. 
        /// Used At: Admin.
        /// </summary>
        /// <param name="name"></param>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("remove-resource-by-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteResourceByNameAsync(string name)
        {
            if (EnvironmentVariables.Env != "dev" && EnvironmentVariables.Env != "stage")
            {
                throw new NotSupportedException();
            }

            await _toolWriteService.DeleteToolByName(name);
            return Ok();
        }


        /// <summary>
        /// Api to get collection of Tools for dropdown, 
        /// Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of tools.</returns>
        [HttpPost("ToolAutoCompleteSearch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ToolAutocompleteModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ToolAutocompleteModel>>> ToolAutoCompleteSearch(bool showOnHomepage = false)
        {

            return await _toolReadService.ToolAutoCompleteSearch(showOnHomepage);
        }


        /// <summary>
        ///  Api to add new Tool, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of newly added tool.</returns>
        [HttpPost("addTool")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ToolViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ToolViewModel>> AddToolAsync([FromForm]AddToolModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);
            return await _toolWriteService.AddToolAsync(model);
        }


        /// <summary>
        ///  Api to remove the Tool entity, 
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
        public async Task<ActionResult> RemoveToolAsync(int id)
        {
            await _toolWriteService.RemoveToolAsync(id);
            return Ok();
        }


        /// <summary>
        ///  Api to get Tool details, 
        ///  Used At: Admin and End-User
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the details of tool.</returns>
        [HttpGet("{id}")]
        [Authorize]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ToolViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ToolViewModel>> GetToolAsync(int id)
        {
            var result = await _toolReadService.GetToolAsync(id);
            if (result != null) return result;

            return NotFound();
        }


        /// <summary>
        ///  Api to edit the Tool entity, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of newly updated tool.</returns>
        [HttpPost("editTool")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ToolViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ToolViewModel>> EditToolsAsync([FromForm] EditToolModel model)
        {
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);
            return await _toolWriteService.EditToolsAsync(model);
        }


        /// <summary>
        ///  Api to update all Tools at Cloud, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the success response.</returns>
        [HttpPost("updateAllToolOnCloud")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<string>> UpdateAllToolOnCloud()
        {
            return await _toolWriteService.UpdateAllToolOnCloud();
        }

        /// <summary>
        /// Api to migrate all db Tools to cloud, 
        /// Used At: Admin.
        /// </summary>
        /// <returns>OK response</returns>
        [HttpPost("migrateDbToolsToCloud")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<string>> MigrateDbToolsToCloud()
        {
            await _toolWriteService.MigrateDbToolsToCloud();
            return Ok();
        }

        /// <summary>
        ///   Api to get export filtered Tools/Resources in Excel, 
        ///   Used At: Admin.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>null</returns>
        [HttpPost("exportResources")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ExportResources (ExportResourceFilter filter)
        {
            await _toolReadService.ExportResource(filter);

            var url = $"https://{EnvironmentVariables.S3BucketMedia}.s3.{EnvironmentVariables.AwsRegion}.amazonaws.com/{EnvironmentVariables.Env}-resources.xls";
            return Ok(url);
        }

        /// <summary>
        ///  Api to get featured Resources, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>An object that contains the collection of featured Resources.</returns>
        [HttpGet("featured-resurces")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<FeaturedResourceModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<FeaturedResourceModel>>> GetFeaturedResourcesAsync()
        {
            return await _toolReadService.GetFeaturedResourcesAsync();
        }
    }
}