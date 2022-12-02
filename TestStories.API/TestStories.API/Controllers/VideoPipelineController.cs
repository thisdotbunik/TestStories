using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;

namespace TestStories.API.Controllers
{
    /// <inheritdoc />
    [Route("api/video/pipeline")]
    [ApiController]
    public class VideoPipelineController : ControllerBase
    {
        private readonly IVideoPipelineService _videoPipelineService;
        private readonly IS3BucketService _s3BucketService;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="videoPipelineService"></param>
        public VideoPipelineController(IVideoPipelineService videoPipelineService,  IS3BucketService s3BucketService )
        {
            _videoPipelineService = videoPipelineService;
            _s3BucketService = s3BucketService;
        }


        /// <summary>
        ///     Customer request S3 presigned URL for Video upload. Formats supported
        /// </summary>
        /// <param name="source">
        ///     The GetPreSignedUrlRequest that defines the
        ///     parameters of the operation.
        /// </param>
        /// <returns>An object that is the signed http request.</returns>
        [HttpPost("presignurl")]
        [Authorize(Roles = "Admin,Admin-Editor,Partner-User,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SignedUrlModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SignedUrlModel>> UploadWithSignedUrl(VideoPipelineSignedUrlRequest source)
        {
            return await _videoPipelineService.UploadWithSignedUrl(source);
        }


        /// <summary>
        ///     Video Pipeline. Get record by id and presing url
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpGet("presignurl/{uuid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SignedUrlModel>> RetrieveWithSignedUrl(string uuid)
        {
            return await _videoPipelineService.RetrieveWithSignedUrl(uuid);
        }


        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost("upload/success")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> VideoUploaded(VideoPipelineEvent source)
        {
            await _videoPipelineService.VideoUploaded(source);
            return Ok();
        }


        /// <summary>
        ///     Video Pipeline errored when transcoded
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Ok response</returns>
        [HttpPost("error")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> TranscodeError(VideoPipelineEventError source)
        {
            await _videoPipelineService.TranscodeError(source);
            return Ok();
        }


        /// <summary>
        ///     Video Pipeline  Event successfully transcoded
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost("success")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> TranscodeSuccess(VideoPipelineEventSuccess source)
        {
            await _videoPipelineService.TranscodeSuccess(source);
            return Ok();
        }


        /// <summary>
        /// Get Thumbnail url from Uuid
        /// </summary>
        /// <param name="imageUuid"></param>
        /// <returns></returns>
        [HttpGet("thumbnailUrl/{imageUuid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SignedUrlModel>> GetThumbnailUrl(string imageUuid)
        {
            var decodedId = System.Web.HttpUtility.UrlDecode(imageUuid);
            var response = !string.IsNullOrEmpty(imageUuid) ? _s3BucketService.RetrieveImageCDNUrl(decodedId) : string.Empty;
            return new SignedUrlModel { Url = response, HttpVerb = HttpVerb.GET };
        }

    }
}