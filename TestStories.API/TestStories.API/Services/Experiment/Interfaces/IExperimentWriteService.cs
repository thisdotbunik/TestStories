using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IExperimentWriteService
    {
        Task<ExperimentViewModel> AddExperimentAsync (AddExperimentModel model, int userId);
        Task<ExperimentViewModel> EditExperimentAsync(int experimentId, EditExperimentModel entity);
        Task<ExperimentViewModel> UpdateExperimentStatusAsync (UpdateExperimentModel model);
        Task TrackEvent(int eventTypeId, long mediaId);
    }
}
