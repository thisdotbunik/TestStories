using System.Linq;
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
    [Route("api/editorPicks")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ServiceFilter(typeof(CustomAuthorizationFilter))]
    public class EditorPicksController : ControllerBase
    {

        private readonly IEditorPickWriteService _editorPickWriteService;
        private readonly IEditorPickReadService _editorPickReadService;
        private readonly IUserReadService _userReadService;
        private int _userId = 0;
        private readonly ILogger<EditorPicksController> _logger;

        /// <inheritdoc />
        public EditorPicksController(IEditorPickWriteService editorPickWriteService , IUserReadService userReadService, ILogger<EditorPicksController> logger, IEditorPickReadService editorPickReadService)
        {
            _editorPickWriteService = editorPickWriteService;
            _editorPickReadService = editorPickReadService;
            _userReadService = userReadService;
            _logger = logger;
        }

        /// <summary>
        ///  Api to get Editor Picks details, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id">
        /// id of the Editor Pick
        /// </param>
        /// <returns>OK Response</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EditorPickModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EditorPickModel>> GetEditorPicksAsync(int id)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Get :request for controller: EditorPicksController and action :  GetEditorPicksAsync for id {id} and userId:{_userId} ");

            var result = await _editorPickReadService.GetEditorPicksAsync(id);
            if(result != null)
            {
                return result;
            }
            return NotFound();
        }


        /// <summary>
        ///  Api to add Editor Picks, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns> An object that contains the details of newly added Editor Pick</returns>
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EditorPickModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<EditorPickModel>> SaveEditorPicksAsync(EditorPicksModel model)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Add:Received request for controller: EditorPicksController and action :  SaveEditorPicksAsync add details {LogsandException.GetCurrentInputJsonString(model)} and userId:{_userId}");
            
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return await _editorPickWriteService.SaveEditorPicksAsync(model);
        }

        /// <summary>
        ///  Api to edit Editor Pick, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id">
        /// id of the Editor Pick
        /// </param>
        /// <param name="model"></param>
        /// <returns>An object that contains the updated details of Editor Pick</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EditorPickModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<EditorPickModel>> EditEditorPicksAsync(int id, EditorPicksModel model)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Update:Receivedrequest for controller:EditorPicksController and action:EditEditorPicksAsync edit details:{LogsandException.GetCurrentInputJsonString(model)} and userId:{_userId}");
            
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return await _editorPickWriteService.EditEditorPicksAsync(id , model);
        }


        /// <summary>
        ///  Api to remove Editor Pick, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="id">
        /// id of the Editor Pick
        /// </param>
        /// <returns> OK Response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemoveEditorPicksAsync(int id)
        {
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            _logger.LogDebug($"Delete: Received request for controller:EditorPicksController and action:RemoveEditorPicksAsync for id:{id} and UserId {_userId}");

            await _editorPickWriteService.RemoveEditorPicksAsync(id);
            return Ok();
        }
    }
}