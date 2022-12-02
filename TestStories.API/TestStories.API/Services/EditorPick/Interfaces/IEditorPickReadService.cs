using System.Threading.Tasks;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IEditorPickReadService
    {
        Task<EditorPickModel> GetEditorPicksAsync (int id);
    }
}
