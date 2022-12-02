using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Concrete
{
    public class EditorPickWriteService: IEditorPickWriteService
    {
        private readonly TestStoriesContext _context;

        /// <inheritdoc />
        public EditorPickWriteService(TestStoriesContext ctx)
        {
            _context = ctx;
        }
        public async Task<EditorPickModel> SaveEditorPicksAsync (EditorPicksModel model)
        {
            var editorPick = await AddEditorPicks(new EditorPicks
            {
                Title = model.Title ,
                EmbeddedCode = model.EmbeddedCode
            });

            if ( editorPick != null )
            {
                var ediotorPicksModel = new EditorPickModel
                {
                    Id = editorPick.Id ,
                    Title = editorPick.Title ,
                    EmbeddedCode = editorPick.EmbeddedCode
                };

                return ediotorPicksModel;
            }

            throw new BusinessException("Can not add editor Picks. Please, try again.");
        }

        public async Task<EditorPickModel> EditEditorPicksAsync (int id , EditorPicksModel model)
        {
            var editorPick = await _context.EditorPicks.SingleOrDefaultAsync(x => x.Id == id);
            if ( editorPick == null )
            {
                throw new BusinessException("EditorPick not found");
            }

            var response = await EditEditorPicks(id , model);
            if ( response != null )
            {
                return new EditorPickModel
                {
                    Id = response.Id ,
                    Title = response.Title ,
                    EmbeddedCode = response.EmbeddedCode
                };
            }

            throw new BusinessException("Can not edit EditorPicks. Please, try again.");
        }

        public async Task RemoveEditorPicksAsync (int id)
        {
            var dbEditorPick = await _context.EditorPicks.SingleOrDefaultAsync(t => t.Id == id);
            if ( dbEditorPick == null )
            {
                throw new BusinessException("EditorPick not found");
            }
            await RemoveEditorPicks(id);
        }

        #region Private Methods
        private async Task<EditorPicks> AddEditorPicks (EditorPicks entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        private async Task<EditorPicks> EditEditorPicks (int id , EditorPicksModel entity)
        {
            var dbEditorPicks = await _context.EditorPicks.SingleOrDefaultAsync(t => t.Id == id);
            if ( dbEditorPicks == null )
                return null;
            dbEditorPicks.Title = entity.Title;
            dbEditorPicks.EmbeddedCode = entity.EmbeddedCode;
            _context.EditorPicks.Update(dbEditorPicks);
            await _context.SaveChangesAsync();
            return dbEditorPicks;
        }

        private async Task RemoveEditorPicks (int id)
        {
            var dbEditorPicks = await _context.EditorPicks.SingleOrDefaultAsync(t => t.Id == id);
            if ( dbEditorPicks != null )
            {
                _context.EditorPicks.Remove(dbEditorPicks);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

    }
}
