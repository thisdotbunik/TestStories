using System;
using System.Threading.Tasks;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface ICommonReadService
    {
        Task<CommonApis> Lookup (LookupType lookupType);
        Task<Page<ContextChange>> GetContentChanges(DateTime? publishedDate, int offset, int limit, bool isFilteredByMediaId);
    }
}
