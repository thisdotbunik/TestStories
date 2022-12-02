using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;
using TestStories.Common;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        readonly IImageMigration _imageMigration;

        public CallbackController(IImageMigration imageMigration)
        {
            _imageMigration = imageMigration;
        }


        /// <summary>
        ///  Action to trigger lambda function to generate resized images for old original images
        /// </summary>
        [HttpPost("image/process-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<BaseResponse>> ProcessAll(EntityType entityType)
        {
            if(entityType == EntityType.None)
            {
                throw new ArgumentException("Entity type cannot be None");
            }

            _imageMigration.ProcessImages(entityType);
            return Ok();
        }
    }
}