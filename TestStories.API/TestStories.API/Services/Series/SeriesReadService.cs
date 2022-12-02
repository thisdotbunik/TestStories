using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public class SeriesReadService : ISeriesReadService
    {
        private readonly TestStoriesContext _context;
        readonly IS3BucketService _s3BucketService;
        /// <inheritdoc />
        public SeriesReadService (TestStoriesContext ctx , IS3BucketService s3BucketService)
        {
            _context = ctx;
            _s3BucketService = s3BucketService;
        }

        public async Task<SeriesModel> GetSeriesAsync (int id)
        {
            var entity = await _context.Series.Include(x => x.SeriesMedia).Where(x => x.Id == id).SingleOrDefaultAsync();
            if (entity != null)
            {
                var seriesMedias = ( from srMedia in entity.SeriesMedia
                                     join media in _context.Media on srMedia.MediaId equals media.Id
                                     select new MediaShortModel
                                     {
                                         Id = media.Id ,
                                         Name = media.Name
                                     }).ToList();

                var response = new SeriesModel
                {
                    Id = entity.Id,
                    SeriesTypeId = entity.SeriestypeId,
                    SeriesTitle = entity.Name,
                    SeriesDescription = entity.Description ,
                    SeriesLogo = !string.IsNullOrEmpty(entity.Logo) ? _s3BucketService.RetrieveImageCDNUrl(entity.Logo) : string.Empty,
                    Logos = await _s3BucketService.GetCompressedImages(entity.Logo , EntityType.Series),
                    SeriesImage = !string.IsNullOrEmpty(entity.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(entity.FeaturedImage) : string.Empty,
                    Images = await _s3BucketService.GetCompressedImages(entity.FeaturedImage , EntityType.Series),
                    HomepageBanner = !string.IsNullOrEmpty(entity.HomepageBanner) ?  _s3BucketService.RetrieveImageCDNUrl(entity.HomepageBanner) : string.Empty,
                    HomepageBanners = await _s3BucketService.GetCompressedImages(entity.HomepageBanner , EntityType.Series),
                    LogoFileName = entity.LogoMetadata,
                    ImageFileName = entity.FeaturedImageMetadata,
                    HomepageBannerName = entity.HomepageBannerMetadata,
                    SuggestedMedias = seriesMedias,
                    SeoUrl = entity.SeoUrl,
                    ShowOnMenu = entity.ShowOnMenu,
                    SeriesDescriptionColor = entity.DescriptionColor,
                    SeriesLogoSize = entity.LogoSize
                };

                return response;
            }
            return null;
        }

        public  async Task<CollectionModel<SeriesViewModel>> GetAllSeriesAsync (SeriesViewRequest request)
        {
            var query = _context.Series.Select(x => new SeriesViewModel
            {
                Id = x.Id,
                SeriesTypeId = x.SeriestypeId,
                SeriesName = x.Name,
                Title = x.Name,
                Description = x.Description,
                SeoUrl = x.SeoUrl,
                ShowOnMenu = x.ShowOnMenu,
                DescriptionColor = x.DescriptionColor,
                LogoSize = x.LogoSize
            });

            if (!string.IsNullOrEmpty(request.FilterString))
            {
                query = query.Where(series => series.Title == request.FilterString);
            }

            if (request.DisplayedOnMenuOnly)
            {
                query = query.Where(series => series.ShowOnMenu);
            }

            query = query.OrderBy(x => x.SeriesName);

            var sortingProperty = request.SortedProperty == null || request.SortedProperty == "" ? "title" : Convert.ToString(request.SortedProperty).ToLower();
            var sortOrder = request.SortOrder == null || request.SortOrder == "" ? "ascending" : Convert.ToString(request.SortOrder).ToLower();

            switch (sortingProperty)
            {
                default:
                    query = sortOrder.ToLower() == "ascending" ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title);
                break;
            }
            var result = new CollectionModel<SeriesViewModel>
            {
                Items = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(),
                TotalCount = await query.CountAsync(),
                PageSize = request.PageSize,
                PageNumber = request.Page
            };

            return result;
        }

        public async Task<CollectionModel<ShortSeriesModel>> GetShortSeriesAsync ()
        {
            var items = await (from x in _context.Series select new ShortSeriesModel
                              {
                                 Id = Convert.ToInt32(x.Id) ,
                                 SeriesTitle = x.Name,
                                 ShowOnMenu = x.ShowOnMenu
                              }).OrderBy(x => x.SeriesTitle).ToListAsync();

            return new CollectionModel<ShortSeriesModel> 
            { 
                Items = items, 
                TotalCount = items.Count 
            };
        }

        public async Task<CollectionModel<SeriesAutoCompleteModel>> SeriesAutoCompleteSearch ()
        {
            var items = await _context.Series.Select(x => new SeriesAutoCompleteModel
            {
                SeriesName = x.Name ,
                Title = x.Name
            }).OrderBy(x => x.SeriesName).ToListAsync();

            return new CollectionModel<SeriesAutoCompleteModel> 
            { 
                Items = items, 
                TotalCount = items.Count 
            };
        }
    }
}
