using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services.Settings.Interfaces;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;
using Newtonsoft.Json;

namespace TestStories.API.Services
{
    public class SettingReadService : ISettingReadService
    {
        private readonly TestStoriesContext _context;
        public SettingReadService (TestStoriesContext context)
        {
            _context = context;
        }
        
        public async Task<FeaturedCarouselSettingsModel> GetFeaturedCarouselAsync ()
        {
            var ids = new List<long>();
            var item = await _context.Setting.SingleOrDefaultAsync(x => x.Name == SettingKeyEnum.FeaturedCarouselSettings.ToString());
            if ( item != null && !string.IsNullOrEmpty(item.Value) )
            {
                var settings = JsonConvert.DeserializeObject<FeaturedCarouselSettingsModel>(item.Value);
                if ( settings.SetByAdmin && settings.Ids != null && settings.Ids.Any() )
                {
                    var publishMediaIds = _context.Media.Where(t => settings.Ids.Contains(t.Id)).Where(x => x.MediastatusId == (int)MediaStatusEnum.Published).Select(p => p.Id);
                    var settingIds = settings.Ids.Where(y => publishMediaIds.Any(z => z.ToString() == y.ToString())).ToList();
                    var newSettings = new FeaturedCarouselSettingsModel() { Ids = settingIds , Randomize = settings.Randomize , SetByAdmin = settings.SetByAdmin };
                    return newSettings;
                }
                return settings;
            }
            return null;
        }

        public async Task<FeaturedSeriesSettingsModel> GetFeaturedResourcesAsync ()
        {
            var item = await _context.Setting.SingleOrDefaultAsync(x =>
               x.Name == SettingKeyEnum.FeaturedResourcesSettings.ToString());
            if ( item != null && !string.IsNullOrEmpty(item.Value) )
            {
                return JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(item.Value);
            }
            return null;
        }

        public async Task<FeaturedSeriesSettingsModel> GetFeaturedSeriesAsync ()
        {
            var item = await _context.Setting.SingleOrDefaultAsync(x =>
               x.Name == SettingKeyEnum.FeaturedSeriesSettings.ToString());
            if ( item != null && !string.IsNullOrEmpty(item.Value) )
            {
                return JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(item.Value);
            }
            return null;
        }

        public async Task<FeaturedTopicsSettingsModel> GetFeaturedTopicsAsync ()
        {
            var item = await _context.Setting.SingleOrDefaultAsync(x =>
                x.Name == SettingKeyEnum.FeaturedTopicsSettings.ToString());
            if ( item != null && !string.IsNullOrEmpty(item.Value) )
            {
                return JsonConvert.DeserializeObject<FeaturedTopicsSettingsModel>(item.Value);
            }
            return null;
        }
    }
}
