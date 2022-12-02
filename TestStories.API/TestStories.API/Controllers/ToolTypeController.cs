using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Filters;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;

namespace TestStories.API.Controllers
{
    [Route("api/toolTypes")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ServiceFilter(typeof(CustomAuthorizationFilter))]
    public class ToolTypeController : ControllerBase
    {
        private readonly IToolTypeWriteService _toolTypeWriteService;
        private readonly IToolTypeReadService _toolTypeReadService;

        /// <inheritdoc />
        public ToolTypeController( IToolTypeReadService toolTypeReadService,IToolTypeWriteService toolTypeWriteService)
        {
            _toolTypeWriteService = toolTypeWriteService;
            _toolTypeReadService = toolTypeReadService;
        }


        /// <summary>
        ///  Api to get tooltypes, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the tool types</returns>
        [HttpPost("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ToolTypeListModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CollectionModel<ToolTypeListModel>>> GetToolTypesAsync(FilterToolViewRequest filter)
        {
            return await _toolTypeReadService.GetToolTypesAsync(filter);
        }


        /// <summary>
        /// Api to get collection of ToolType for dropdown, 
        /// Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of tool types.</returns>
        [HttpGet("autoCompleteSearch")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ToolTypeAutoComplete>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ToolTypeAutoComplete>>> ToolTypeAutoCompleteSearch()
        {
            return await _toolTypeReadService.ToolTypeAutoCompleteSearch();
        }

        /// <summary>
        /// Api to get collection of Active ToolType, 
        /// Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of Active tool types.</returns>
        [HttpGet("activeToolTypes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ToolTypeListModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ToolTypeListModel>>> GetActiveToolTypesAsync()
        {
            return await _toolTypeReadService.GetActiveToolTypesAsync();
        }

        /// <summary>
        ///  Api to add new Tool Type, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of newly added tool type.</returns>
        [HttpPost("addType")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ToolTypeModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ToolTypeModel>> AddToolTypeAsync([FromForm]AddToolType model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _toolTypeWriteService.AddToolTypeAsync(model);
            return Ok(result);
        }

        /// <summary>
        ///  Api to add new Tool Type, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of newly added tool type.</returns>
        [HttpPut("editType")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ToolTypeModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ToolTypeModel>> EditToolTypeAsync(int toolTypeId, [FromForm]AddToolType model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _toolTypeWriteService.EditToolTypeAsync(toolTypeId, model);
            return Ok(result);
        }

        /// <summary>
        ///  Api to remove the Tool Type entity, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OK Response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemoveToolTypeAsync(int id)
        {
            await _toolTypeWriteService.RemoveToolTypeAsync(id);
            return Ok();
        }

        /// <summary>
        ///  Api to enable the Tool Type entity, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OK Response</returns>
        [HttpPut("enableToolType")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> EnableToolTypeAsync(int id)
        {
            await _toolTypeWriteService.EnableToolTypeAsync(id);
            return Ok();
        }


        /// <summary>
        /// Removes ToolType entity by name. 
        /// Used At: Admin.
        /// </summary>
        /// <param name="name"></param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("remove-resource-type-by-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteToolTypeByNameAsync(string name)
        {
            if (EnvironmentVariables.Env != "dev" && EnvironmentVariables.Env != "stage")
            {
                throw new NotSupportedException();
            }
            await _toolTypeWriteService.DeleteToolTypeByNameAsync(name);
            return Ok();
        }

        
    }
}