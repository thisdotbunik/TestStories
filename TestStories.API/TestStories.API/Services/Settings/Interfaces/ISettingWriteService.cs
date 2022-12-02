using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public interface ISettingWriteService
    {
        Task<FeaturedCarouselSettingsModel> SaveFeaturedCarouselAsync (SaveFeaturedCarouselSettingsModel model);
        Task<FeaturedTopicsSettingsModel> SaveFeaturedTopicsAsync (SaveFeaturedTopicsSettingsModel model);
        Task<FeaturedSeriesSettingsModel> SaveFeaturedSeriesAsync (SaveFeaturedSeriesSettingsModel model);
        Task<FeaturedSeriesSettingsModel> SaveFeaturedResourcesAsync (SaveFeaturedSeriesSettingsModel model);
        Task<EditorPicks> AddEditorPicks(EditorPicks entity);
        Task<EditorPicks> EditEditorPicks(int id, EditorPicksModel entity);
        Task RemoveEditorPicks(int id);

        Task ChangePartnerOrder(PartnerOrderModel entities);

        Task ResetPartnerOrder();
    }
}
