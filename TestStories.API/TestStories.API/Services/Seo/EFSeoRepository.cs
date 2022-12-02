using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Concrete
{
    public class EFSeoRepository : ISeoRepository
    {
        private readonly TestStoriesContext _context;

        public EFSeoRepository(TestStoriesContext ctx)
        {
            _context = ctx;
        }
        public async Task<SeoModel> AddEditSeoTagAsync(SeoTagModel model)
        {
            Seotag seoTag;
            seoTag = await AddEditSeoTag(new Seotag
            {
                EntityId = model.EntityId,
                SeotagtypeId = model.EntityTypeId,
                TitleTag = model.TitleTag,
                H1 = model.H1,
                H2 = model.H2,
                PrimaryKeyword = model.PrimaryKeyword,
                SecondaryKeyword = model.SecondaryKeyword,
                MetaDescription = model.MetaDescription,
                PageDescription = model.PageDescription
            });
            if (seoTag == null)
            {
                throw new BusinessException("Can not assign tag. Please, try again.");
            }

            return new SeoModel
            {
                EntityId = seoTag.EntityId,
                EntityTypeId = seoTag.SeotagtypeId,
                TitleTag = seoTag.TitleTag,
                H1 = seoTag.H1,
                H2 = seoTag.H2,
                PrimaryKeyword = seoTag.PrimaryKeyword,
                SecondaryKeyword = seoTag.SecondaryKeyword,
                MetaDescription = seoTag.MetaDescription,
                PageDescription = seoTag.PageDescription
            };
        }

        public async Task<SeoModel> GetSeoTagInfoAsync(long entityId, int entityTypeId)
        {
            var query = _context.Seotag.Select(x => new SeoModel
            {
                EntityId = x.EntityId,
                EntityTypeId = x.SeotagtypeId,
                TitleTag = x.TitleTag,
                PrimaryKeyword = x.PrimaryKeyword,
                SecondaryKeyword = x.SecondaryKeyword,
                H1 = x.H1,
                H2 = x.H2,
                MetaDescription = x.MetaDescription,
                PageDescription = x.PageDescription
            }).Where(x => x.EntityId == entityId && x.EntityTypeId == entityTypeId);

            return await query.SingleOrDefaultAsync();
        }

        private async Task<Seotag> AddEditSeoTag(Seotag entity)
        {
            var seoTag=await _context.Seotag.SingleOrDefaultAsync(tag=>tag.EntityId==entity.EntityId && tag.SeotagtypeId==entity.SeotagtypeId);
            if (seoTag != null)
            {
                seoTag.TitleTag = entity.TitleTag;
                seoTag.PrimaryKeyword = entity.PrimaryKeyword;
                seoTag.SecondaryKeyword = entity.SecondaryKeyword;
                seoTag.MetaDescription = entity.MetaDescription;
                seoTag.PageDescription = entity.PageDescription;
                seoTag.H1 = entity.H1;
                seoTag.H2 = entity.H2;
                _context.Update(seoTag);
                _context.SaveChanges();
                return seoTag;
            }
            else
            {
                await _context.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
        }
    }
}
