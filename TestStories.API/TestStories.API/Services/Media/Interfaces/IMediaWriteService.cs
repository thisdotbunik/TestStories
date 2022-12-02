using System.Collections.Generic;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IMediaWriteService
    {
        Task<BaseResponse> UpdateMediaStatus(int mediaId, byte statusId);
        Task<MediaShortModel> AddMediaAsync(AddMediaModel model, int userId);
        Task<MediaShortModel> EditMediaAsync(EditMediaModel model, int userId);
        Task DeleteMediaByTitle(string title);
        Task<BaseResponse> ArchiveMediaAsync(int mediaId, int userId, string role);
        Task<BaseResponse> UnarchiveMediaAsync(int mediaId, int userId, string role);
        Task MigrateSrtFiles(List<AddSrtFileModel> srtFiles);
        Task GenerateSeoFriendlyUrl(bool isAllUpdate);
        Task GenerateHlsUrl ();
        Task UpdateMediaSeoUrl ();
        Task<List<MediaSeoDetailModel>> GenerateNewSeoUrl ();
        Task<List<MediaSeoDetailModel>> GetUpdatedSeoUrl ();
        Task UpdateAllMediaOnCloud();
        Task<PartnerMediaModel> SendToPartnerAsync(AddSendToPartnerModel model);
        Task DeleteMediaById(long id);
        Task UpdateMediaUniqueIds(string fileName);
    }
}
