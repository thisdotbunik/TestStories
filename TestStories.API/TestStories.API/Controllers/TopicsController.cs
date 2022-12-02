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
    /// <inheritdoc />
    [Route("api/topics")]
    [ApiController]
    public class TopicsController : ControllerBase
    {
        private readonly ITopicWriteService _topicWriteService;
        private readonly ITopicReadService _topicReadService; 

        /// <inheritdoc />
        public TopicsController(ITopicWriteService topicWriteService, ITopicReadService topicReadService)
        {
            _topicWriteService = topicWriteService;
            _topicReadService = topicReadService;
        }


        /// <summary>
        ///  Api to get the collection of filtered Topics, 
        ///  Used At: Admin and End-User. 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>An object that contains the collection of filtered topics.</returns>
        [HttpPost("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<TopicViewModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<TopicViewModel>>> GetTopicViewCollectionAsync(
            FilterTopicViewRequest filter)
        {
            return await _topicReadService.GetTopicViewCollectionAsync(filter);
        }


        /// <summary>
        ///  Api to get collection of Topics for dropdown, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>An object that contains the collection of Topics.</returns>
        [HttpPost("TopicAutoCompleteSearch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<TopicAutoCompleteModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<TopicAutoCompleteModel>>> TopicAutoCompleteSearch()
        {
            return await _topicReadService.TopicAutoCompleteSearch();
        }


        /// <summary>
        ///  Api to get collection of Topics, 
        ///  Used At: Admin and End-User.
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [HttpGet("fullInfo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<TopicModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<TopicModel>>> GetTopicsAsync([FromQuery] int? parentId)
        {
            return await _topicReadService.GetTopicsAsync(parentId);
        }


        /// <summary>
        ///  Api to get details of Topic entity, 
        ///  Used At: Admin and End-User.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An object that contains the details of topic.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TopicModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TopicModel>> GetTopicAsync(int id)
        {
            var response = await _topicReadService.GetTopicAsync(id);
            if ( response!= null )
            {
                return response;
            }
            return NotFound();
        }


        /// <summary>
        ///  Api to get collection of Topic short info, 
        ///  Used At: Admin and End-User.
        /// </summary>
        /// <returns>An object that contains the collection of topics short info.</returns>
        [HttpGet("shortInfo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionModel<ShortTopicModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionModel<ShortTopicModel>>> GetShortTopicsAsync()
        {
            return await _topicReadService.GetShortTopicsAsync();
        }


        /// <summary>
        ///  Api to add a Topic entity, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of newly added topic. </returns>
        [HttpPost("addTopic")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TopicModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<TopicModel>> AddTopicAsync([FromForm] AddTopicModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return await _topicWriteService.AddTopicAsync(model);
        }


        /// <summary>
        ///  Api to edit the Topic entity, 
        ///  Used At: Admin.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>An object that contains the details of updated topic</returns>
        [HttpPost("editTopic")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TopicModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<TopicModel>> EditTopicAsync([FromForm] EditTopicModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return await _topicWriteService.EditTopicAsync(model);
        }


        /// <summary>
        ///  Api to remove the Topic entity, 
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
        public async Task<ActionResult> RemoveTopicAsync(int id)
        {
            await _topicWriteService.RemoveTopicAsync(id);   
            return Ok();
        }

        /// <summary>
        ///  Api to update all Topics on Cloud, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>OK Response</returns>
        [HttpPost("updateCloudTopics")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCloudTopics()
        {
            await _topicWriteService.UpdateCloudTopics();
            return Ok();
        }

        /// <summary>
        /// Api to migrate all db Topics on Cloud,
        /// Used At: Admin.
        /// </summary>
        /// <returns>OK Response</returns>
        [HttpPost("migrateDbTopicsToCloud")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> MigrateDbTopicsToCloud()
        {
            await _topicWriteService.MigrateDbTopicsToCloud();
            return Ok();
        }


        /// <summary>
        /// Removes Topic entity by name. 
        /// Used At: Admin.
        /// </summary>
        /// <param name="name"></param>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("remove-by-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteTopicByNameAsync(string name)
        {
            if (EnvironmentVariables.Env != "dev" && EnvironmentVariables.Env != "stage")
            {
                throw new NotSupportedException();
            }

            await _topicWriteService.DeleteTopicByName(name);
            return Ok();
        }
       
    }
}