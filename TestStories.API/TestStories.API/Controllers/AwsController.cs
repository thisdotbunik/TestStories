using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Controllers
{
    [Route("api/aws")]
    [ApiController]
    public class AwsController : ControllerBase
    {
        private readonly ILogger<AwsController> _logger;

        public AwsController(ILogger<AwsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Api to log errors at AWS
        /// </summary>
        /// <param name="_model"></param>
        /// <returns>OK Response</returns>
        [HttpPost("LogError")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> LogError(LogErrorExceptionModel _model)
        {
            _logger.LogError($"Error: Message: {_model.Exception}, StackTrace: {_model.StackTrace}");
            return Ok();
        }

    }
}