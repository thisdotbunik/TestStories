using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IEditorPickWriteService
    {
        Task<EditorPickModel> SaveEditorPicksAsync (EditorPicksModel model);
        Task<EditorPickModel> EditEditorPicksAsync (int id , EditorPicksModel model);
        Task RemoveEditorPicksAsync (int id);
    }
}
