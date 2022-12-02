using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IToolTypeReadService
    {
        Task<CollectionModel<ToolTypeListModel>> GetToolTypesAsync(FilterToolViewRequest filter);
        Task<CollectionModel<ToolTypeAutoComplete>> ToolTypeAutoCompleteSearch();
        Task<CollectionModel<ToolTypeListModel>> GetActiveToolTypesAsync();
    }
}
