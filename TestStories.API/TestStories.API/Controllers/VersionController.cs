using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TestStories.Common;

namespace TestStories.API.Controllers
{
    [ApiController]
    public class VersionController : Controller
    {
        [HttpGet]
        [Route("probz")]
        public IActionResult GetProb()
        {
            return Ok();
        }

        [HttpGet]
        [Route("versionz")]
        public IActionResult GetVersion()
        {
            return StatusCode(200,
                new Dictionary<string, string>
                {
                    {"application", $"{EnvironmentVariables.ServiceName}"},
                    {"version", $"{EnvironmentVariables.ServiceVersion}"},
                    {"environment", $"{EnvironmentVariables.Env}"},
                    {"region", $"{EnvironmentVariables.AwsRegion}"}
                });
        }
    }
}