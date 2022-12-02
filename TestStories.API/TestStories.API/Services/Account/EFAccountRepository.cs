using System.Threading.Tasks;
using TestStories.API.Services;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Concrete
{
    public class EFAccountRepository : IAccountRepository
    {
        private readonly TestStoriesContext _context;

        /// <inheritdoc />
        public EFAccountRepository(TestStoriesContext ctx)
        {
            _context = ctx;
        }

        public async Task<User> UserSigUp(User entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
