using System.Collections.Generic;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface ICommonWriteService
    {
        Task MigrateSrtFiles (List<AddSrtFileModel> model);
        Task GenerateSeoFriendlyUrl (bool isAllUpdate);
        Task ContactUsMail (ContactUsMailModel mailModel);
        Task BecomeAPartnerMail (BecomeAPartnerMailModel mailModel);
        Task<NewletterResponse> SubscribeNewsletter (int userId , SubscribeNewletterModel model);
        Task GenerateMediaSiteMap ();
        Task ExportMedias(MediaFilter filter);
        Task<byte[]> ExportUsersSubscribedData();
        void FixVideoDuration();
        Task FixVideoSize ();
        Task FixAudioSize ();

        //Task UpdateMediaUniqueIds(string fileName);
    }
}
