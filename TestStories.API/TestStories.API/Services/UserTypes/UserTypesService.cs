using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public class UserTypesService : IUserTypesService
    {
        readonly TestStoriesContext _context;

        public UserTypesService(TestStoriesContext context)
        {
            _context = context;
        }
        public async Task<List<UserTypeModel>> GetUserTypesAsync()
        {
            var items = await _context.UserType.Select(x => new UserTypeModel { 
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();
            if (items == null || items.Count == 0)
            {
                throw new BusinessException("UserTypes not found");
            }
            return items;
        }
    }
}
