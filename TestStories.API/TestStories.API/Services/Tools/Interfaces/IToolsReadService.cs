using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IToolsReadService
    {
        Task<CollectionModel<ShortToolModel>> GetToolViewCollectionAsync (FilterToolViewRequest filter);
        Task<CollectionModel<ShortToolModel>> GetClientTools(FilterClientToolViewRequest filter);
        Task<CollectionModel<ToolItemOutput>> GetResources(FilterClientToolViewRequest request);
        Task<CollectionModel<ToolAutocompleteModel>> ToolAutoCompleteSearch (bool showOnHomepage = false);
        Task<ToolViewModel> GetToolAsync (int id);
        Task ExportResource (ExportResourceFilter filter);
        Task<CollectionModel<FeaturedResourceModel>> GetFeaturedResourcesAsync();
    }
}
