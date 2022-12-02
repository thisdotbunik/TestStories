using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestStories.API.Services
{
    public interface ISeriesStandaloneReadService
    {
        Task<IEnumerable<dynamic>> GetSeriesStandaloneAsync(string fields, string seriesIds);
    }
}
