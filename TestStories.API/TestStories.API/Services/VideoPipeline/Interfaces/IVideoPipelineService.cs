using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IVideoPipelineService
    {
        Task<SignedUrlModel> UploadWithSignedUrl(VideoPipelineSignedUrlRequest source);
        Task<SignedUrlModel> RetrieveWithSignedUrl(string uuid);
        Task VideoUploaded(VideoPipelineEvent source);
        Task TranscodeError(VideoPipelineEventError source);
        Task TranscodeSuccess(VideoPipelineEventSuccess source);
        Task<SignedUrlModel> GetThumbnailUrl(string imageUuid);
    }
}
