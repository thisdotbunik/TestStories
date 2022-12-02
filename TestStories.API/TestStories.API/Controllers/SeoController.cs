using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Filters;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Controllers
{
    [Route("api/seo")]
    [ApiController]
    public class SeoController : ControllerBase
    {
        private readonly ISeoRepository _repository;

        /// <inheritdoc />
        public SeoController(ISeoRepository repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// AccessibleBy: Admin Users,
        ///               Authenticated Users
        /// Description:  Api to add a SeoTag entity.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of newly added SeoTag. </returns>
        [HttpPost("addSeotag")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SeoModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SeoModel>> AddSeoTagAsync([FromForm] SeoTagModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return await _repository.AddEditSeoTagAsync(model);
        }

        /// <summary>
        /// AccessibleBy: Admin Users,
        ///               End Users,
        ///               Authenticated Users,
        ///               Unauthenticated Users
        /// Description:  Api to get seoTag entity.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityTypeId"></param>
        /// <returns>An object that contains the details of seoTag.</returns>
        [HttpGet("seoTagInfo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SeoModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SeoModel>> GetSeoTagInfoAsync(long entityId, int entityTypeId)
        {
            var result = await _repository.GetSeoTagInfoAsync(entityId, entityTypeId);
            if (result != null) return result;
            return NotFound();
        }
    }
}