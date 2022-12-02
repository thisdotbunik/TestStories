using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IExperimentReadService
    {
        Task<CollectionModel<ExperimentListModel>> FilterExperimentAsync (ExperimentFilterRequest experimentFilter);
        Task<CollectionModel<ExperimentAutoComplete>> ExperiementAutoCompleteAsync ();
        Task<ExperimentViewModel> GetExperiementAsync (int id);
    }
}
