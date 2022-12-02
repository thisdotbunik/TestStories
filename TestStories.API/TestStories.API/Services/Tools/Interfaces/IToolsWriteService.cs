using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IToolsWriteService
    {
        Task DeleteToolByName(string name);
        Task<ToolViewModel> AddToolAsync (AddToolModel model);
        Task<ToolViewModel> EditToolsAsync (EditToolModel model);
        Task RemoveToolAsync(int topicId);
        Task<string> UpdateAllToolOnCloud ();
        Task MigrateDbToolsToCloud ();
    }
}
