using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api/userTypes")]
    [ApiController]
    public class UserTypesController : ControllerBase
    {
        private readonly IUserTypesService _repository;
        private readonly ILogger<UserTypesController> _logger;
        /// <inheritdoc />
        public UserTypesController(IUserTypesService repo, ILogger<UserTypesController> logger)
        {
            _repository = repo;
            _logger = logger;
        }


       /// <summary>
       /// Api to get user types master data, 
       /// Used At: Admin
       /// </summary>
       /// <returns>An object that contains the collection of user types.</returns>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTypeModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<UserTypeModel>>> GetUserTypesAsync()
        {
            return await _repository.GetUserTypesAsync();
        }
    }
}