using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface ISeoRepository
    {
        Task<SeoModel> GetSeoTagInfoAsync(long entityId, int entityTypeId);
        Task<SeoModel> AddEditSeoTagAsync(SeoTagModel model);
    }
}
