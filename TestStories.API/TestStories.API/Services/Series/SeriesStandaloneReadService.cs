using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public class SeriesStandaloneReadService : ISeriesStandaloneReadService
    {
        private const string All = "all";
        private readonly TestStoriesContext _context;
        readonly IS3BucketService _s3BucketService;

        public SeriesStandaloneReadService(TestStoriesContext ctx, IS3BucketService s3BucketService)
        {
            _context = ctx;
            _s3BucketService = s3BucketService;
        }

        public async Task<IEnumerable<dynamic>> GetSeriesStandaloneAsync(string fields, string seriesIds)
        {
            bool allFields = fields.Equals(All, System.StringComparison.OrdinalIgnoreCase);
            bool allIds = seriesIds.Equals(All, System.StringComparison.OrdinalIgnoreCase);

            string[] fieldNames = fields.Split(",").Select(c=> c.Trim()).ToArray();
            int[] ids = allIds ? new int[] { }  : seriesIds.Split(",").Select(c=> int.Parse(c.Trim())).ToArray();

            return await GetSeriesAsync(ids, fieldNames, allFields, allIds);
        }

        public async Task<dynamic> GetSeriesAsync(int[] ids, string[] fields, bool allFields, bool allIds)
        {
            List<dynamic> result = new List<dynamic>();

            var entities = await _context.Series.Include(x => x.SeriesMedia).Where(x => allIds || ids.Contains(x.Id)).ToListAsync();

            foreach (var entity in entities)
            {
                var seriesMedias = (from srMedia in entity.SeriesMedia
                                    join media in _context.Media on srMedia.MediaId equals media.Id
                                    select new MediaShortModel
                                    {
                                        Id = media.Id,
                                        Name = media.Name
                                    }).ToList();

                dynamic response = new System.Dynamic.ExpandoObject();
                
                if(allFields || fields.Contains("Id", StringComparer.OrdinalIgnoreCase))
                {
                    response.id = entity.Id;
                }
                if (allFields || fields.Contains("SeriesTypeId", StringComparer.OrdinalIgnoreCase))
                {
                    response.seriesTypeId = entity.SeriestypeId;
                }
                
                if (allFields || fields.Contains("SeriesTitle", StringComparer.OrdinalIgnoreCase))
                {
                    response.seriesTitle = entity.Name;
                }
               
                if (allFields || fields.Contains("SeriesDescription", StringComparer.OrdinalIgnoreCase))
                {
                    response.seriesDescription = entity.Description;
                }
                
                if (allFields || fields.Contains("SeriesLogo", StringComparer.OrdinalIgnoreCase))
                {
                    response.seriesLogo = !string.IsNullOrEmpty(entity.Logo) ? _s3BucketService.RetrieveImageCDNUrl(entity.Logo) : string.Empty;
                }
                
                if (allFields || fields.Contains("Logos", StringComparer.OrdinalIgnoreCase))
                {
                    response.logos = await _s3BucketService.GetCompressedImages(entity.Logo, EntityType.Series);
                }
                
                if (allFields || fields.Contains("SeriesImage", StringComparer.OrdinalIgnoreCase))
                {
                    response.seriesImage = !string.IsNullOrEmpty(entity.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(entity.FeaturedImage) : string.Empty;
                }
               
                if (allFields || fields.Contains("Images", StringComparer.OrdinalIgnoreCase))
                {
                    response.images = await _s3BucketService.GetCompressedImages(entity.FeaturedImage, EntityType.Series);
                }
                
                if (allFields || fields.Contains("HomepageBanner", StringComparer.OrdinalIgnoreCase))
                {
                    response.homepageBanner = !string.IsNullOrEmpty(entity.HomepageBanner) ? _s3BucketService.RetrieveImageCDNUrl(entity.HomepageBanner) : string.Empty;
                }

                if (allFields || fields.Contains("HomepageBanners", StringComparer.OrdinalIgnoreCase))
                {
                    response.homepageBanners = await _s3BucketService.GetCompressedImages(entity.HomepageBanner, EntityType.Series);
                }
                
                if (allFields || fields.Contains("LogoFileName", StringComparer.OrdinalIgnoreCase))
                {
                    response.logoFileName = entity.LogoMetadata;
                }

                if (allFields || fields.Contains("ImageFileName", StringComparer.OrdinalIgnoreCase))
                {
                    response.imageFileName = entity.FeaturedImageMetadata;
                }

                if (allFields || fields.Contains("HomepageBannerName", StringComparer.OrdinalIgnoreCase))
                {
                    response.homepageBannerName = entity.HomepageBannerMetadata;
                }

                if (allFields || fields.Contains("SuggestedMedias", StringComparer.OrdinalIgnoreCase))
                {
                    response.suggestedMedias = seriesMedias;
                }

                if (allFields || fields.Contains("SeoUrl", StringComparer.OrdinalIgnoreCase))
                {
                    response.seoUrl = entity.SeoUrl;
                }

                if (allFields || fields.Contains("ShowOnMenu", StringComparer.OrdinalIgnoreCase))
                {
                    response.showOnMenu = entity.ShowOnMenu;
                }

                if (allFields || fields.Contains("SeriesDescriptionColor", StringComparer.OrdinalIgnoreCase))
                {
                    response.seriesDescriptionColor = entity.DescriptionColor;
                }

                if (allFields || fields.Contains("SeriesLogoSize", StringComparer.OrdinalIgnoreCase))
                {
                    response.seriesLogoSize = entity.LogoSize;
                }

                result.Add(response);
                
            }
            return result;
        }
    }
}
