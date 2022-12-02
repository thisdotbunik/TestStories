using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;
using Newtonsoft.Json;

namespace TestStories.API.Concrete
{
    /// <inheritdoc />
    public class SettingWriteService : ISettingWriteService
    {
        private readonly TestStoriesContext _context;

        /// <inheritdoc />
        public SettingWriteService(TestStoriesContext ctx)
        {
            _context = ctx;
        }

        public async Task<EditorPicks> AddEditorPicks(EditorPicks entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<EditorPicks> EditEditorPicks(int id, EditorPicksModel entity)
        {
            var dbEditorPicks = await _context.EditorPicks.SingleOrDefaultAsync(t => t.Id == id);
            if (dbEditorPicks == null) return null;
            dbEditorPicks.Title = entity.Title;
            dbEditorPicks.EmbeddedCode = entity.EmbeddedCode;
            _context.EditorPicks.Update(dbEditorPicks);
            await _context.SaveChangesAsync();
            return dbEditorPicks;
        }

        public async Task RemoveEditorPicks(int id)
        {
            var dbEditorPicks = await _context.Tool.SingleOrDefaultAsync(t => t.Id == id);
            if (dbEditorPicks != null)
            {
                _context.Tool.Remove(dbEditorPicks);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ChangePartnerOrder(PartnerOrderModel entities)
        {
            var lstPartners = new List<Partner>();
            foreach (var item in entities.PartnersOrdering)
            {
                var partner = await _context.Partner.SingleOrDefaultAsync(x => x.Id == item.PartnerId);
                if (partner != null)
                {
                    var totalPartner=await _context.Partner.Where(x=>x.OrderNumber==partner.OrderNumber).CountAsync();
                    if (totalPartner == 1)
                    {
                        partner.OrderNumber = item.OrderNumber;
                        lstPartners.Add(partner);
                    }
                }
            }
            if (lstPartners.Count > 0)
            {
                _context.Partner.UpdateRange(lstPartners);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ResetPartnerOrder()
        {
            var partners = new List<Partner>();
            var dbPartners = await _context.Partner.OrderBy(x => x.Id).ToListAsync();
            for (var i = 0; i < dbPartners.Count; i++)
            {
                var partner = await _context.Partner.SingleOrDefaultAsync(x => x.Id == dbPartners[i].Id);
                if (partner != null)
                {
                    partner.OrderNumber = i + 1;
                    partners.Add(partner);
                }
            }
            if (partners.Count > 0)
            {
                _context.Partner.UpdateRange(partners);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<FeaturedCarouselSettingsModel> SaveFeaturedCarouselAsync (SaveFeaturedCarouselSettingsModel model)
        {
            Setting settings;

            var item = await _context.Setting.SingleOrDefaultAsync(x =>
                x.Name == SettingKeyEnum.FeaturedCarouselSettings.ToString());

            if ( item != null )
                settings = await EditSettingAsync(new Setting
                {
                    Id = item.Id ,
                    Name = SettingKeyEnum.FeaturedCarouselSettings.ToString() ,
                    Value = JsonConvert.SerializeObject(model)
                });
            else
                settings = await AddSettingAsync(new Setting
                {
                    Name = SettingKeyEnum.FeaturedCarouselSettings.ToString() ,
                    Value = JsonConvert.SerializeObject(model)
                });

            if ( settings != null && !string.IsNullOrEmpty(settings.Value) )
            {
                return JsonConvert.DeserializeObject<FeaturedCarouselSettingsModel>(settings.Value);
            }
            return null;
        }

        public async Task<FeaturedTopicsSettingsModel> SaveFeaturedTopicsAsync (SaveFeaturedTopicsSettingsModel model)
        {
            Setting settings;

            var item = await _context.Setting.SingleOrDefaultAsync(x => x.Name == SettingKeyEnum.FeaturedTopicsSettings.ToString());

            if ( item != null )
                settings = await EditSettingAsync(new Setting
                {
                    Id = item.Id ,
                    Name = SettingKeyEnum.FeaturedTopicsSettings.ToString() ,
                    Value = JsonConvert.SerializeObject(model)
                });
            else
                settings = await AddSettingAsync(new Setting
                {
                    Name = SettingKeyEnum.FeaturedTopicsSettings.ToString() ,
                    Value = JsonConvert.SerializeObject(model)
                });

            if ( settings != null && !string.IsNullOrEmpty(settings.Value) )
            {
                return JsonConvert.DeserializeObject<FeaturedTopicsSettingsModel>(settings.Value);
            }
            return null;
        }

        public async Task<FeaturedSeriesSettingsModel> SaveFeaturedSeriesAsync (SaveFeaturedSeriesSettingsModel model)
        {
            Setting settings;

            var item = await _context.Setting.SingleOrDefaultAsync(x => x.Name == SettingKeyEnum.FeaturedSeriesSettings.ToString());

            if ( item != null )
                settings = await EditSettingAsync(new Setting
                {
                    Id = item.Id ,
                    Name = SettingKeyEnum.FeaturedSeriesSettings.ToString() ,
                    Value = JsonConvert.SerializeObject(model)
                });
            else
                settings = await AddSettingAsync(new Setting
                {
                    Name = SettingKeyEnum.FeaturedSeriesSettings.ToString() ,
                    Value = JsonConvert.SerializeObject(model)
                });

            if ( settings != null && !string.IsNullOrEmpty(settings.Value) )
            {
                return JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(settings.Value);
            }
            return null;
        }

        public async Task<FeaturedSeriesSettingsModel> SaveFeaturedResourcesAsync (SaveFeaturedSeriesSettingsModel model)
        {
            Setting settings;

            var item = await _context.Setting.SingleOrDefaultAsync(x => x.Name == SettingKeyEnum.FeaturedResourcesSettings.ToString());

            if ( item != null )
                settings = await EditSettingAsync(new Setting
                {
                    Id = item.Id ,
                    Name = SettingKeyEnum.FeaturedResourcesSettings.ToString() ,
                    Value = JsonConvert.SerializeObject(model)
                });
            else
                settings = await AddSettingAsync(new Setting
                {
                    Name = SettingKeyEnum.FeaturedResourcesSettings.ToString() ,
                    Value = JsonConvert.SerializeObject(model)
                });

            if ( settings != null && !string.IsNullOrEmpty(settings.Value) )
            {
                return JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(settings.Value);
            }
            return null;
        }

        #region Private Methods

        private async Task<Setting> AddSettingAsync(Setting entity)
        {
            var dbSetting = await _context.Setting.SingleOrDefaultAsync(t => t.Name.Trim() == entity.Name.Trim());
            if (dbSetting != null) return null;
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        private async Task<Setting> EditSettingAsync(Setting entity)
        {
            var dbSetting = await _context.Setting.SingleOrDefaultAsync(t => t.Name.Trim() == entity.Name.Trim());
            if (dbSetting == null) return null;
            dbSetting.Value = entity.Value;
            _context.Setting.Update(dbSetting);
            await _context.SaveChangesAsync();

            return dbSetting;
        }

        #endregion

    }
}