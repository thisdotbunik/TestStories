using System.Threading.Tasks;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public interface IAccountRepository
    {
        Task<User> UserSigUp(User entity);
    }
}
