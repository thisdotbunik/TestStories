using System.Threading.Tasks;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services.Settings.Interfaces
{
    public interface ISettingReadService
    {
        Task<FeaturedCarouselSettingsModel> GetFeaturedCarouselAsync ();
        Task<FeaturedTopicsSettingsModel> GetFeaturedTopicsAsync ();
        Task<FeaturedSeriesSettingsModel> GetFeaturedSeriesAsync ();
        Task<FeaturedSeriesSettingsModel> GetFeaturedResourcesAsync ();
    }
}

