using System.Collections.Generic;
using System.Threading.Tasks;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IUserTypesService
    {
        Task<List<UserTypeModel>> GetUserTypesAsync();
    }
}
