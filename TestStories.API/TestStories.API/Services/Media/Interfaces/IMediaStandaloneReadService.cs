using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestStories.API.Services
{
    public interface IMediaStandaloneReadService
    {
        Task<string> GetMediaDownloadUrlStandaloneAsync(int id);
        Task<IEnumerable<dynamic>> GetMediaStandaloneAsync(string mediaTypes, string ids, string fields);
    }
}
