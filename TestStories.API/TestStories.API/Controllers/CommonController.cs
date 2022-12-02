using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using TestStories.Common;

namespace TestStories.API.Controllers
{

    [Route("api/common")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        readonly ICommonReadService _commonReadService;
        readonly ICommonWriteService _commonWriteService;
        readonly IUserReadService _userReadService;
        readonly ILogger<CommonController> _logger;
        int _userId = 0;


        public CommonController (ICommonReadService commonReadService , IUserReadService userReadService, ICommonWriteService commonWriteService ,
            ILogger<CommonController> logger)

        {
            _logger = logger;
            _commonReadService = commonReadService;
            _commonWriteService = commonWriteService;
            _userReadService = userReadService;
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("fixVideoDuration")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Page<ContextChange>))]
        public ActionResult FixVideoDuration()
        {
            _commonWriteService.FixVideoDuration();
            return Ok();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("fixVideoSize")]
        [ProducesResponseType(StatusCodes.Status200OK )]
        public async Task<ActionResult> FixVideoSize ()
        {
             await _commonWriteService.FixVideoSize();
            return Ok();
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("fixAudioSize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> FixAudioSize ()
        {
            await _commonWriteService.FixAudioSize();
            return Ok();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("generateMediaSiteMap")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(Page<ContextChange>))]
        public async Task<ActionResult> GenerateMediaSiteMap ()
        {
             await _commonWriteService.GenerateMediaSiteMap();
            return Ok();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Admin-Editor")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpPost("exportMedias")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ExportMedias(MediaFilter filter)
        {
            await _commonWriteService.ExportMedias(filter);
            
            var url= $"https://{EnvironmentVariables.S3BucketMedia}.s3.{EnvironmentVariables.AwsRegion}.amazonaws.com/{EnvironmentVariables.Env}-media.xls";
            return Ok(url);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("get-content-changes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<SeoModel>> GetContentChanges(DateTime? publishedDate, int offset, int limit, bool isFilteredByMediaId = false)
        {
            if ( !publishedDate.HasValue )
            {
                throw new ValidationException("Published date should be specified");
            }
            var result = await _commonReadService.GetContentChanges(publishedDate.Value.Date, offset, limit, isFilteredByMediaId);
            return Ok(result);
        }


        /// <summary>
        ///  Api to get master data like SeriesType, Tags, Source etc, 
        ///  Used At: Admin and End-User.
        /// </summary>
        /// <param name="lookupType">
        /// </param>
        /// <returns>An object that contains the master data</returns>
        [HttpPost("Lookup")]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(CommonApis))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommonApis>> Lookup (LookupType lookupType)
        {
            var jsonString = LogsandException.GetCurrentInputJsonString(lookupType);
            _logger.LogDebug($"Get:Received request for commoncontroller and action:lookup input details:{jsonString}");
            return await _commonReadService.Lookup(lookupType);
        }


        /// <summary>
        ///  Api to migrate SrtFiles to MediaSrt table, 
        ///  Used At: Admin.
        /// </summary>
        /// <returns>Ok Response</returns>
        [HttpPost("migrateSrtFiles")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> MigrateSrtFiles (List<AddSrtFileModel> model)
        {
            _logger.LogDebug($"Update:Request received Controller:CommonController and Action:migrateSrtFiles");

            await _commonWriteService.MigrateSrtFiles(model);
            return Ok();
        }

        /// <summary>
        ///  Api to genearte SEO friendly urls, 
        ///  Used At: Admin. 
        /// </summary>
        /// <returns> OK Response</returns>
        [HttpPost("generateSeoFriendlyUrl")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> GenerateSeoFriendlyUrl (bool isAllUpdate)
        {
            _logger.LogDebug($"Update:Request received Controller:CommonController and Action:generateSeoFriendlyUrl");
            await _commonWriteService.GenerateSeoFriendlyUrl(isAllUpdate);
            return Ok();
        }

        /// <summary>
        ///  Api to send the Contact Us mail to user, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns>OK Response</returns>
        [HttpPost("contactUsMail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ContactUsMail (ContactUsMailModel mailModel)
        {
            await _commonWriteService.ContactUsMail(mailModel);
            return Ok();
        }


        /// <summary>
        ///  Api to send Become A Partner mail, 
        ///  Used At: End-User.
        /// </summary>
        /// <returns></returns>
        [HttpPost("becomeAPartnerMail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> BecomeAPartnerMail (BecomeAPartnerMailModel mailModel)
        {
            await _commonWriteService.BecomeAPartnerMail(mailModel);
            return Ok();
        }

        /// <summary>
        ///  Api to Subscribe Newsletter,
        ///  Used At: End-User.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>An object that contains the user details</returns>
        [HttpPost("subscribeNewsletter")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK , Type = typeof(NewletterResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<NewletterResponse>> SubscribeNewsletter (SubscribeNewletterModel model)
        {
            if ( !ModelState.IsValid )
            {
                _logger.LogError($"http model not validated");
                return BadRequest(ModelState);
            }
            _userId = await _userReadService.GetUserIdByEmail(this.CurrentUserEmail());
            var response = await _commonWriteService.SubscribeNewsletter(_userId , model);
            return Ok(response);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Admin-Editor")]
        [ServiceFilter(typeof(CustomAuthorizationFilter))]
        [HttpGet("exportUsersSubscribedData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ExportUsersSubscribedData()
        {
            var content = await _commonWriteService.ExportUsersSubscribedData();
            return File(content ,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ,
                        "UserSubscribedData.xlsx");
        }
    }
}
