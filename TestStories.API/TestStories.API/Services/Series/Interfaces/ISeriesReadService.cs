using System.Collections.Generic;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface ISeriesReadService
    {
        Task<CollectionModel<ShortSeriesModel>> GetShortSeriesAsync ();
        Task<CollectionModel<SeriesAutoCompleteModel>> SeriesAutoCompleteSearch ();
        Task<SeriesModel> GetSeriesAsync (int id);
        Task<CollectionModel<SeriesViewModel>> GetAllSeriesAsync (SeriesViewRequest request);
    }
}
