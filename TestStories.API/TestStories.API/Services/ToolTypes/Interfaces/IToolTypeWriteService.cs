using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IToolTypeWriteService
    {
        Task<ToolTypeModel> AddToolTypeAsync(AddToolType model);
        Task<ToolTypeModel> EditToolTypeAsync(int toolTypeId, AddToolType model);
        Task RemoveToolTypeAsync(int id);
        Task DeleteToolTypeByNameAsync(string name);
        Task EnableToolTypeAsync(int id);
    }
}
