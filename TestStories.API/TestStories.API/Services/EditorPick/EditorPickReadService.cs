using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public class EditorPickReadService : IEditorPickReadService
    {
        private readonly TestStoriesContext _context;
        public EditorPickReadService (TestStoriesContext context)
        {
            _context = context;
        }

        public async Task<EditorPickModel> GetEditorPicksAsync (int id)
        {
            var result = await _context.EditorPicks.SingleOrDefaultAsync(x => x.Id == id);
            if ( result != null )
            {
                var response = new EditorPickModel
                {
                    Id = result.Id ,
                    Title = result.Title ,
                    EmbeddedCode = result.EmbeddedCode
                };
                return response;
            }
            return null;
        }
    }
}
