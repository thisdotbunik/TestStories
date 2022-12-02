using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public interface ISeriesWriteService
    {
        Task DeleteSeriesByName(string name);

        Task<SeriesModel> AddSeriesAsync(AddSeriesModel entity);

        Task<SeriesModel> EditSeriesAsync(EditSeriesModel entity);

        Task RemoveSeriesAsync(int seriesId);

        Task UpdateAsync(Series entity);

        Task UpdateCloudSeries ();

        Task MigrateDbSeriesToCloud ();
    }
}
