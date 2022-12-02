using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.CloudSearch.Common.Constants;
using TestStories.CloudSearch.Service.Interface;
using TestStories.CloudSearch.Service.Model;
using TestStories.Common;
using TestStories.Common.Configurations;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;
using Newtonsoft.Json;
using WordPressPCL;
using WordPressPCL.Utility;

namespace TestStories.API.Services
{
    public class HomePageReadService : IHomePageReadService
    {
        readonly TestStoriesContext _context;
        readonly IS3BucketService _s3BucketService;
        readonly ICloudTopicToolSeriesProvider _cloudTopicToolSeriesProvider;
        readonly ICloudMediaSearchProvider _mediaCloudSearch;
        readonly AppSettings _appSettings;
        readonly ILogger<HomePageReadService> _logger;

        public HomePageReadService(TestStoriesContext context, IS3BucketService s3BucketService, ICloudTopicToolSeriesProvider topicToolSeriesCloudSearch, ICloudMediaSearchProvider mediaCloudSearch, IOptions<AppSettings> appSettings, ILogger<HomePageReadService> logger)
        {
            _context = context;
            _s3BucketService = s3BucketService;
            _cloudTopicToolSeriesProvider = topicToolSeriesCloudSearch;
            _mediaCloudSearch = mediaCloudSearch;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public async Task<AllInOne> GetAll()
        {

            return new AllInOne
            {
                FeaturedBlogs = (await GetOptimisedFeaturedBlogsAsync()).Select(x => new OptimisedBlogResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    FeaturedImage = x.FeaturedImage,
                    Url = x.Url
                }).ToList(),
                FeaturedResources = (await GetOptimisedFeaturedResources()).Select(x => new AllFeaturedResponse<int>
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    FeaturedImage = x.FeaturedImage,
                    SeoUrl = x.SeoUrl
                }).ToList(),
                FeaturedSeries = (await GetOptimisedFeaturedSeries()).Select(x => new OptimisedSeriesRequest
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    FeaturedImage = x.FeaturedImage,
                    SeriesType = x.SeriesType,
                    SeoUrl = x.SeoUrl,
                    MediaTypeId = x.MediaTypeId
                }).ToList(),
                FeaturedTopics = (await GetOptimisedFeaturedTopics()).Select(x => new AllFeaturedResponse<int>
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    FeaturedImage = x.FeaturedImage,
                    SeoUrl = x.SeoUrl
                }).ToList(),
                FeaturedVideos = (await GetOptimisedFeaturedVideos()).Select(x => new OptimisedVideosResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    FeaturedImage = x.FeaturedImage,
                    SeoUrl = x.SeoUrl,
                    SeriesType = x.SeriesType,
                    Series = x.Series,
                    Topic = x.Topic,
                    Url=x.Url,
                    MediaTypeId=x.MediaTypeId,
                    UniqueId = x.UniqueId,
                    MediaDuration = x.MediaDuration,
                    ResourceTitle = x.ResourceTitle,
                    ResourceUrl = x.ResourceUrl
                }).ToList()
            };

        }
        public async Task<CollectionModel<FeaturedResourceModel>> FilteredResourcesAsync(FilterRequestModel model)
        {
            var tools = _cloudTopicToolSeriesProvider.ResoureExactCloudSearch(model.Types, model.Partners, model.Topics, model.PageSize, model.PageNumber, model.Sort, model.Order, out var toolCount);

            var result = (from item in tools
                          select new FeaturedResourceModel
                          {
                              Id = Convert.ToInt32(item.id.Replace("Tool", "")),
                              Name = item.title,
                              Description = item.description,
                              Url = item.link,
                              Thumbnail = item.featuredimage,
                              Type = item.type,
                              Partner = item.partner,
                              Topics = item.topics,
                              ShowOnMenu = Convert.ToBoolean(item.showonmenu),
                              ShowOnHomePage = Convert.ToBoolean(item.showonhomepage)
                          }).ToList();

            foreach (var tool in result)
            {
                var thumbnail = tool.Thumbnail;
                tool.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                tool.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Tools);
            }

            return new CollectionModel<FeaturedResourceModel> { Items = result, TotalCount = toolCount, PageNumber = model.PageNumber, PageSize = model.PageSize };
        }
        public async Task<CollectionModel<FilteredSeriesResponse>> FilteredSeriesAsync(SeriesFilterRequest request)
        {
            var query = _context.Series.Include(x => x.Seriestype).Include(z => z.SeriesMedia).Include(m => m.Media).Select(x => new FilteredSeriesResponse
            {
                Id = x.Id,
                Title = x.Name,
                Description = x.Description,
                Type = x.Seriestype.Name,
                SeoUrl = x.SeoUrl,
                FeaturedImage = x.FeaturedImage,
                LatestPublishedMediaDate = x.Media.OrderByDescending(y => y.DatePublishedUtc).FirstOrDefault().DatePublishedUtc
            }).OrderByDescending(x => x.LatestPublishedMediaDate).ThenBy(y => y.Title);

            var totalCount = await query.CountAsync();

            //var sortOrder = request.Order == null || request.Order == "" ? "asc" : request.Order.ToLower();
            //query = sortOrder.ToLower() == "desc" ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title);
            var items = await query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync();

           await Task.Run(() =>
            {
                Parallel.ForEach(items , async series =>
               {
                series.FeaturedImage =  (await _s3BucketService.GetCompressedImages(series.FeaturedImage , EntityType.Series)).Grid;
               });
            });

            return new CollectionModel<FilteredSeriesResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };
        }

        public async Task<CollectionModel<EditorPickModel>> GetEditorPicksAsync()
        {
            var response = await _context.EditorPicks.Select(x => new EditorPickModel
            {
                Id = x.Id,
                Title = x.Title,
                EmbeddedCode = x.EmbeddedCode
            }).ToListAsync();

            if (response != null)
            {
                if (response.Any())
                    return new CollectionModel<EditorPickModel> { Items = response, TotalCount = response.Count };
            }

            return new CollectionModel<EditorPickModel>();
        }

        public async Task<CollectionModel<BlogResponse>> GetFeaturedBlogsAsync()
        {
            var result = (from item in await DynamoDBService.GetBlogs()
                          select new BlogResponse
                          {
                              Id = item.BlogId,
                              Title = item.Title,
                              Description = item.Description,
                              PublishedDate = item.PublishedDate,
                              FeaturedImage = item.FeaturedImage,
                              Url = item.Url,
                              FetchedAt = item.FetchedAt
                          }).OrderByDescending(x => x.PublishedDate).Take(4).ToList();

            return new CollectionModel<BlogResponse> { Items = result, TotalCount = result.Count };
        }

        public async Task<List<OptimisedBlogResponse>> GetOptimisedFeaturedBlogsAsync()
        {
            var result = (from item in await DynamoDBService.GetBlogs()
                          select new OptimisedBlogResponse
                          {
                              Id = item.BlogId,
                              Title = item.Title,
                              Description = item.Description,
                              FeaturedImage = item.FeaturedImage,
                              PublishedDate = item.PublishedDate,
                              Url = item.Url
                          }).OrderByDescending(x => x.PublishedDate).Take(4).ToList();

            return result;
        }

        public async Task<CollectionModel<FeaturedResourceModel>> GetFeaturedResourcesAsync()
        {
            var ids = new List<int>();
            var settingInfo = await _context.Setting.SingleOrDefaultAsync(setting => setting.Name == SettingKeyEnum.FeaturedResourcesSettings.ToString());
            var settings = settingInfo != null ? JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(settingInfo.Value) : null;

            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.Value))
            {
                if (settings != null)
                {
                    if (settings.SetByAdmin && settings.Ids != null)
                        ids.AddRange(settings.Ids.ToList());

                    if (settings.Randomize)
                    {
                        var toolIds = await _context.Tool.Select(tool => tool.Id).ToListAsync();
                        if (toolIds.Count > 0)
                        {
                            var random = new Random();
                            var minIndex = toolIds.Min();
                            var maxIndex = toolIds.Max();
                            ids.AddRange(toolIds
                                .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex))).ToList());
                        }
                    }
                }
            }
            else
            {
                var toolIds = await _context.Tool.Select(tool => tool.Id).ToListAsync();
                if (toolIds.Count > 0)
                {
                    var random = new Random();
                    var minIndex = toolIds.Min();
                    var maxIndex = toolIds.Max();
                    ids.AddRange(toolIds
                        .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex))).ToList());
                }
            }

            var result =  (from tool in _context.Tool.Include(x => x.Tooltype).Include(x => x.Partner).AsEnumerable().Where(x => ids.Contains(x.Id)).ToList()
                           let topics = (from p in _context.ToolTopic.Where(x => x.ToolId == tool.Id).ToList()
                                              join q in _context.Topic
                                              on p.TopicId equals q.Id
                                              select q.Name).ToList()
                                select new FeaturedResourceModel
                                {
                                    Id = tool.Id,
                                    Name = tool.Name,
                                    TypeId = tool.Tooltype?.Id,
                                    Type = tool.Tooltype?.Name,
                                    PartnerId = tool.Partner?.Id,
                                    Partner = tool.Partner?.Name,
                                    Description = tool.Description,
                                    Url = tool.Url,
                                    Topics = topics.Any() ? topics.ToList() : null,
                                    Thumbnail = tool.FeaturedImage,
                                    ShowOnHomePage = tool.ShowOnHomepage
                                }).OrderBy(d => ids.IndexOf(d.Id)).Take(6).ToList();

            foreach (var tool in result)
            {
                var thumbnail = tool.Thumbnail;
                tool.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                tool.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Tools);
            }

            if (result.Count > 0)
                return new CollectionModel<FeaturedResourceModel> { Items = result, TotalCount = result.Count };
            return new CollectionModel<FeaturedResourceModel>();
        }

        public async Task<List<AllFeaturedResponse<int>>> GetOptimisedFeaturedResources()
        {
            var ids = new List<int>();
            var settingInfo = await _context.Setting.SingleOrDefaultAsync(setting => setting.Name == SettingKeyEnum.FeaturedResourcesSettings.ToString());
            var settings = settingInfo != null ? JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(settingInfo.Value) : null;

            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.Value))
            {
                if (settings != null)
                {
                    if (settings.SetByAdmin && settings.Ids != null)
                        ids.AddRange(settings.Ids.ToList());

                    if (settings.Randomize)
                    {
                        var toolIds = await _context.Tool.Select(tool => tool.Id).ToListAsync();
                        if (toolIds.Count > 0)
                        {
                            var random = new Random();
                            var minIndex = toolIds.Min();
                            var maxIndex = toolIds.Max();
                            ids.AddRange(toolIds
                                .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex))).ToList());
                        }
                    }
                }
            }
            else
            {
                var toolIds = await _context.Tool.Select(tool => tool.Id).ToListAsync();
                if (toolIds.Count > 0)
                {
                    var random = new Random();
                    var minIndex = toolIds.Min();
                    var maxIndex = toolIds.Max();
                    ids.AddRange(toolIds
                        .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex))).ToList());
                }
            }

            var result =  (from tool in _context.Tool.AsEnumerable().Where(x => ids.Contains(x.Id)).ToList()
                                select new AllFeaturedResponse<int>
                                {
                                    Id = tool.Id,
                                    Title = tool.Name,
                                    Description = tool.Description,
                                    FeaturedImage = tool.FeaturedImage,
                                    SeoUrl = tool.Url
                                }).OrderBy(d => ids.IndexOf(d.Id)).Take(6).ToList();
            await Task.Run(() =>
            {
                Parallel.ForEach(result , async tool =>
               {
                tool.FeaturedImage =  ( await _s3BucketService.GetCompressedImages(tool.FeaturedImage , EntityType.Tools) ).Grid;
               });
            });

            if (result.Count > 0)
                return result;
            return new List<AllFeaturedResponse<int>>();
        }

        public async Task<CollectionModel<SuggestedSeriesMediaModel>> GetFeaturedSeriesAsync()
        {
            var ids = new List<int>();
            var settingInfo = await _context.Setting.SingleOrDefaultAsync(setting => setting.Name == SettingKeyEnum.FeaturedSeriesSettings.ToString());
            var settings = settingInfo != null ? JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(settingInfo.Value) : null;

            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.Value))
            {
                if (settings != null)
                {
                    if (settings.SetByAdmin && settings.Ids != null)
                        ids.AddRange(settings.Ids);

                    if (settings.Randomize)
                    {
                        var seriesIds = await _context.Series.Select(series => series.Id).ToListAsync();
                        if (seriesIds.Count > 0)
                        {
                            var random = new Random();
                            var minIndex = seriesIds.Min();
                            var maxIndex = seriesIds.Max();
                            ids.AddRange(seriesIds
                                .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex))));
                        }
                    }
                }
            }
            else
            {
                var seriesIds = await _context.Series.Select(series => series.Id).ToListAsync();
                if (seriesIds.Count > 0)
                {
                    var random = new Random();
                    var minIndex = seriesIds.Min();
                    var maxIndex = seriesIds.Max();
                    ids.AddRange(seriesIds
                        .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex))));
                }
            }

            var result =  _context.Series.Include(x => x.Seriestype).Include(x => x.Media).AsEnumerable().Where(series => ids.Contains(series.Id)).Select(
                      series => new SuggestedSeriesMediaModel
                      {
                          Id = series.Id,
                          SeriesTitle = series.Name,
                          SeriesTypeId = series.Seriestype.Id,
                          SeriesType = series.Seriestype.Name,
                          SeriesDescription = series.Description,
                          SeriesLogo = series.Logo,
                          SeriesImage = series.FeaturedImage,
                          HomepageBanner = string.IsNullOrEmpty(series.HomepageBanner) ? series.FeaturedImage : series.HomepageBanner,
                          SeoUrl = series.SeoUrl,
                          Medias = series.Media
                      }).OrderBy(d => ids.IndexOf(d.Id)).ToList();

            foreach (var series in result)
            {
                series.Medias = series.Medias.Where(x => x.MediastatusId == (int)MediaStatusEnum.Published &&
                                           !x.IsDeleted && !x.IsPrivate && x.MediatypeId != (int)MediaTypeEnum.Banner &&
                                          (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                            x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                           x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc)).ToList();

                var lstAllMedia = new List<DataAccess.Entities.Media>();


                foreach (var curItem in series.Medias)
                {
                    if (curItem.ActiveFromUtc > curItem.DatePublishedUtc)
                    {
                        curItem.DateCreatedUtc = Convert.ToDateTime(curItem.ActiveFromUtc);
                    }
                    else
                    {
                        curItem.DateCreatedUtc = Convert.ToDateTime(curItem.DatePublishedUtc);
                    }
                    lstAllMedia.Add(curItem);
                }

                lstAllMedia = lstAllMedia.OrderByDescending(media => media.DateCreatedUtc).ToList();
                var recentMedia = lstAllMedia.FirstOrDefault();
                series.VideoCount = lstAllMedia.Count;
                series.VideoLink = recentMedia?.Url;
                series.VideoThumbnail = recentMedia?.FeaturedImage;
                series.MediaId = recentMedia?.Url != null ? recentMedia.Id : 0;
                series.MediaTypeId = recentMedia?.Url != null ? recentMedia.MediatypeId : 0;
            }

            foreach (var series in result)
            {
                series.Medias = null;
                var logo = series.SeriesLogo;
                series.SeriesLogo = !string.IsNullOrEmpty(logo) ? _s3BucketService.RetrieveImageCDNUrl(logo) : string.Empty;
                series.SeriesLogos = await _s3BucketService.GetCompressedImages(logo, EntityType.Series);

                var image = series.SeriesImage;
                series.SeriesImage = !string.IsNullOrEmpty(image) ? _s3BucketService.RetrieveImageCDNUrl(image) : string.Empty;
                series.SeriesImages = await _s3BucketService.GetCompressedImages(image, EntityType.Series);

                var banner = series.HomepageBanner;
                series.HomepageBanner = !string.IsNullOrEmpty(banner) ? _s3BucketService.RetrieveImageCDNUrl(banner) : string.Empty;
                series.HomepageBanners = await _s3BucketService.GetCompressedImages(banner, EntityType.Series);

                var videoThumbnail = series.VideoThumbnail;
                series.VideoThumbnail = !string.IsNullOrEmpty(videoThumbnail) ? _s3BucketService.RetrieveImageCDNUrl(videoThumbnail) : string.Empty;
                series.VideoThumbnails = await _s3BucketService.GetCompressedImages(videoThumbnail, EntityType.Media);
            }

            if (result.Count > 0)
                return new CollectionModel<SuggestedSeriesMediaModel> { Items = result, TotalCount = result.Count };
            return new CollectionModel<SuggestedSeriesMediaModel>();
        }

        public async Task<List<OptimisedSeriesResponse>> GetOptimisedFeaturedSeries()
        {
            var ids = new List<int>();
            var settingInfo = await _context.Setting.SingleOrDefaultAsync(setting => setting.Name == SettingKeyEnum.FeaturedSeriesSettings.ToString());
            var settings = settingInfo != null ? JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(settingInfo.Value) : null;

            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.Value))
            {
                if (settings != null)
                {
                    if (settings.SetByAdmin && settings.Ids != null)
                        ids.AddRange(settings.Ids.ToList());

                    if (settings.Randomize)
                    {
                        var seriesIds = await _context.Series.Select(series => series.Id).ToListAsync();
                        if (seriesIds.Count > 0)
                        {
                            var random = new Random();
                            var minIndex = seriesIds.Min();
                            var maxIndex = seriesIds.Max();
                            ids.AddRange(seriesIds
                                .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex))).ToList());
                        }
                    }
                }
            }
            else
            {
                var seriesIds = await _context.Series.Select(series => series.Id).ToListAsync();
                if (seriesIds.Count > 0)
                {
                    var random = new Random();
                    var minIndex = seriesIds.Min();
                    var maxIndex = seriesIds.Max();
                    ids.AddRange(seriesIds
                        .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex))));
                }
            }

            var result =  _context.Series.Include(x => x.Seriestype).Include(x => x.Media).ThenInclude(x => x.Mediatype).AsEnumerable().Where(series => ids.Contains(series.Id)).ToList().Select(
                      series => new OptimisedSeriesResponse
                      {
                          Id = series.Id,
                          Title = series.Name,
                          Description = series.Description,
                          FeaturedImage = series.FeaturedImage,
                          SeoUrl = series.SeoUrl,
                          Medias = series.Media,
                          SeriesType = series.Seriestype.Name
                      }).OrderBy(d => ids.IndexOf(d.Id)).ToList();

            foreach (var series in result)
            {
                series.Medias = series.Medias.AsEnumerable().Where(x => x.MediastatusId == (int)MediaStatusEnum.Published &&
                                           !x.IsDeleted && !x.IsPrivate && x.MediatypeId != (int)MediaTypeEnum.Banner &&
                                          (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                            x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                           x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc)).ToList();

                var lstAllMedia = new List<DataAccess.Entities.Media>();


                foreach (var curItem in series.Medias)
                {
                    if (curItem.ActiveFromUtc > curItem.DatePublishedUtc)
                    {
                        curItem.DateCreatedUtc = Convert.ToDateTime(curItem.ActiveFromUtc);
                    }
                    else
                    {
                        curItem.DateCreatedUtc = Convert.ToDateTime(curItem.DatePublishedUtc);
                    }
                    lstAllMedia.Add(curItem);
                }

                lstAllMedia = lstAllMedia.OrderByDescending(media => media.DateCreatedUtc).ToList();
                var recentMedia = lstAllMedia.FirstOrDefault();
                series.MediaTypeId = recentMedia?.Url != null ? recentMedia.MediatypeId : 0;
            }

            await Task.Run(() =>
            {
                Parallel.ForEach(result , async series =>
                {
                    series.FeaturedImage =  ( await _s3BucketService.GetCompressedImages(series.FeaturedImage , EntityType.Series) ).Thumbnail;
                });
            });

            if (result.Count > 0)
                return result;
            return new List<OptimisedSeriesResponse>();
        }

        public async Task<CollectionModel<TopicInfoModel>> GetFeaturedTopicsAsync()
        {
            const int maxRandomTopicItems = 6;
            var ids = new List<int>();
            var settingInfo = await _context.Setting.SingleOrDefaultAsync(x =>
                x.Name == SettingKeyEnum.FeaturedTopicsSettings.ToString());
            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.Value))
            {
                var settings = JsonConvert.DeserializeObject<FeaturedTopicsSettingsModel>(settingInfo.Value);
                if (settings != null)
                {
                    if (settings.SetByAdmin && settings.Ids != null)
                        ids.AddRange(settings.Ids.ToList());

                    if (settings.Randomize)
                    {
                        var topicIds = await _context.Topic.OrderBy(x => x.Id).Select(item => item.Id)
                            .ToListAsync();
                        if (topicIds.Count > 0)
                        {
                            var random = new Random();
                            var minIndex = topicIds.Min();
                            var maxIndex = topicIds.Max();
                            ids.AddRange(topicIds
                                .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex)))
                                .Take(maxRandomTopicItems).OrderBy(i => i).ToList());
                        }
                    }
                }
            }
            else
            {
                var topicIds = await _context.Topic.OrderBy(x => x.Id).Select(item => item.Id)
                    .ToListAsync();
                if (topicIds.Count > 0)
                {
                    var random = new Random();
                    var minIndex = topicIds.Min();
                    var maxIndex = topicIds.Max();
                    ids.AddRange(topicIds
                        .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex)))
                        .Take(maxRandomTopicItems).OrderBy(i => i).ToList());
                }
            }

            var result = (from topic in _context.Topic.AsEnumerable().Where(x => ids.Contains(x.Id)).ToList()
                          let medias = (from p in _context.MediaTopic.ToList()
                                        where p.TopicId == topic.Id
                                        join x in _context.Media on p.MediaId equals x.Id
                                        where x.MediastatusId == (int)MediaStatusEnum.Published && !x.IsDeleted
                                        && x.MediatypeId != (int)MediaTypeEnum.Banner && !x.IsPrivate
                                        && (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                         x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                        x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc)
                                        select x).ToList()
                          select new TopicInfoModel
                          {
                              Id = topic.Id,
                              TopicName = topic.Name,
                              Description = topic.Description,
                              TopicThumbnail = topic.Logo,
                              SeoUrl = topic.SeoUrl,
                              Medias = medias,
                          }).OrderBy(d => ids.IndexOf(d.Id)).ToList();

            foreach (var topic in result)
            {
                topic.VideoCount = topic.Medias.Count;
                var topicThumbnail = topic.TopicThumbnail;
                topic.TopicThumbnail = !string.IsNullOrEmpty(topicThumbnail) ? _s3BucketService.RetrieveImageCDNUrl(topicThumbnail) : string.Empty;
                topic.TopicThumbnails = await _s3BucketService.GetCompressedImages(topicThumbnail, EntityType.Topics);
                topic.Medias = null;
            }

            if (result.Count > 0)
                return new CollectionModel<TopicInfoModel> { Items = result, TotalCount = result.Count };

            return new CollectionModel<TopicInfoModel>();
        }

        public async Task<List<AllFeaturedResponse<int>>> GetOptimisedFeaturedTopics()
        {
            const int maxRandomTopicItems = 6;
            var ids = new List<int>();
            var settingInfo = await _context.Setting.SingleOrDefaultAsync(x =>
                x.Name == SettingKeyEnum.FeaturedTopicsSettings.ToString());
            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.Value))
            {
                var settings = JsonConvert.DeserializeObject<FeaturedTopicsSettingsModel>(settingInfo.Value);
                if (settings != null)
                {
                    if (settings.SetByAdmin && settings.Ids != null)
                        ids.AddRange(settings.Ids.ToList());

                    if (settings.Randomize)
                    {
                        var topicIds = await _context.Topic.OrderBy(x => x.Id).Select(item => item.Id)
                            .ToListAsync();
                        if (topicIds.Count > 0)
                        {
                            var random = new Random();
                            var minIndex = topicIds.Min();
                            var maxIndex = topicIds.Max();
                            ids.AddRange(topicIds
                                .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex)))
                                .Take(maxRandomTopicItems).OrderBy(i => i).ToList());
                        }
                    }
                }
            }
            else
            {
                var topicIds = await _context.Topic.OrderBy(x => x.Id).Select(item => item.Id)
                    .ToListAsync();
                if (topicIds.Count > 0)
                {
                    var random = new Random();
                    var minIndex = topicIds.Min();
                    var maxIndex = topicIds.Max();
                    ids.AddRange(topicIds
                        .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex)))
                        .Take(maxRandomTopicItems).OrderBy(i => i).ToList());
                }
            }

            var result = (from topic in _context.Topic.AsEnumerable().Where(x => ids.Contains(x.Id)).ToList()
                          select new AllFeaturedResponse<int>
                          {
                              Id = topic.Id,
                              Title = topic.Name,
                              Description = topic.Description,
                              FeaturedImage = topic.Logo,
                              SeoUrl = topic.SeoUrl
                          }).OrderBy(d => ids.IndexOf(d.Id)).ToList();

            await Task.Run(() =>
            {
                Parallel.ForEach(result , async topic =>
                {
                    topic.FeaturedImage =  ( await _s3BucketService.GetCompressedImages(topic.FeaturedImage , EntityType.Topics) ).Grid;
                });
            });

            if (result.Count > 0)
                return result;

            return new List<AllFeaturedResponse<int>>();
        }

        public async Task<CollectionModel<FeaturedVideosModel>> GetFeaturedVideosAsync()
        {
            var ids = new List<long>();
            const int maxRandomMediaItems = 4;
            var settingInfo = await _context.Setting.SingleOrDefaultAsync(x =>
                x.Name == SettingKeyEnum.FeaturedCarouselSettings.ToString());
            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.Value))
            {
                var settings = JsonConvert.DeserializeObject<FeaturedCarouselSettingsModel>(settingInfo.Value);
                if (settings != null)
                {
                    if (settings.SetByAdmin && settings.Ids != null)
                    {
                        foreach (var mediaId in settings.Ids.ToList())
                        {
                            var isActive = _context.Media.Any(x => x.Id == mediaId && x.MediastatusId == (int)MediaStatusEnum.Published &&
                                           !x.IsDeleted && !x.IsPrivate &&
                                          (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                            x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                           x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc));

                            if (isActive)
                            {
                                ids.Add(mediaId);
                            }
                        }

                    }
                    if (settings.Randomize)
                    {
                        var mediaIds = _context.Media.Where(x => x.MediastatusId == (int)MediaStatusEnum.Published &&
                                           !x.IsDeleted && !x.IsPrivate &&
                                          (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                            x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                           x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc)).OrderBy(x => x.Id)
                                           .Select(y => y.Id).ToList();

                        if (mediaIds.Any())
                        {
                            var random = new Random();
                            var minIndex = mediaIds.Min();
                            var maxIndex = mediaIds.Max();
                            ids.AddRange(mediaIds
                                .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex)))
                                .Take(maxRandomMediaItems).OrderBy(i => i).ToList());
                        }
                    }
                }
            }
            else
            {
                var mediaIds = _context.Media.Where(x => x.MediastatusId == (int)MediaStatusEnum.Published &&
                                         !x.IsDeleted && !x.IsPrivate &&
                                        (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                          x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                         x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc))
                                          .Select(y => y.Id).ToList();

                if (mediaIds.Any())
                {
                    var random = new Random();
                    var minIndex = mediaIds.Min();
                    var maxIndex = mediaIds.Max();
                    ids.AddRange(mediaIds
                        .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex)))
                        .Take(maxRandomMediaItems).OrderBy(i => i).ToList());
                }
            }

            var result = (from media in _context.Media.Include(y => y.Series).ThenInclude(z => z.Seriestype).AsEnumerable().Where(x => ids.Contains(x.Id)).ToList()
                          let topic = (from mtp in _context.MediaTopic.ToList()
                                       join tp in _context.Topic on mtp.TopicId equals tp.Id into tpGroup
                                       from topic in tpGroup.DefaultIfEmpty()
                                       where mtp.MediaId == media.Id
                                       select topic.Name).FirstOrDefault()
                          select new FeaturedVideosModel
                          {
                              Id = media.Id,
                              Title = media.Name,
                              Description = media.Description,
                              Url = media.Url,
                              Thumbnail = media.Thumbnail,
                              FeaturedImage = media.FeaturedImage,
                              SeriesId = media.SeriesId,
                              Logo = media.Series?.Logo,
                              Series = media.Series?.Name,
                              MediaTypeId = media.MediatypeId,
                              IsSharingAllowed = media.IsSharingAllowed,
                              SeoUrl = media.SeoUrl,
                              UniqueId = media.UniqueId,
                              SeriesType = media.Series?.Seriestype?.Name,
                              Topic = topic
                          }).OrderBy(d => ids.IndexOf(d.Id)).ToList();


            foreach (var media in result)
            {
                var thumbnail = media.Thumbnail;
                media.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                media.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Media);

                var featuredImage = media.FeaturedImage;
                media.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                media.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Media);

                var logo = media.Logo;
                media.Logo = !string.IsNullOrEmpty(logo) ? _s3BucketService.RetrieveImageCDNUrl(logo) : string.Empty;
                media.Logos = await _s3BucketService.GetCompressedImages(logo, EntityType.Media);
            }

            if (result.Count > 0)
            {
                return new CollectionModel<FeaturedVideosModel> { Items = result, TotalCount = result.Count };
            }
            return new CollectionModel<FeaturedVideosModel>();
        }

        public async Task<List<OptimisedVideosResponse>> GetOptimisedFeaturedVideos()
        {
            var ids = new List<long>();
            const int maxRandomMediaItems = 4;
            var settingInfo = await _context.Setting.SingleOrDefaultAsync(x =>
                x.Name == SettingKeyEnum.FeaturedCarouselSettings.ToString());
            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.Value))
            {
                var settings = JsonConvert.DeserializeObject<FeaturedCarouselSettingsModel>(settingInfo.Value);
                if (settings != null)
                {
                    if (settings.SetByAdmin && settings.Ids != null)
                    {
                        foreach (var mediaId in settings.Ids.ToList())
                        {
                            var isActive = _context.Media.Any(x => x.Id == mediaId && x.MediastatusId == (int)MediaStatusEnum.Published &&
                                           !x.IsDeleted && !x.IsPrivate &&
                                          (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                            x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                           x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc));

                            if (isActive)
                            {
                                ids.Add(mediaId);
                            }
                        }

                    }
                    if (settings.Randomize)
                    {
                        var mediaIds = _context.Media.Where(x => x.MediastatusId == (int)MediaStatusEnum.Published &&
                                           !x.IsDeleted && !x.IsPrivate &&
                                          (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                            x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                           x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc)).OrderBy(x => x.Id)
                                           .Select(y => y.Id).ToList();

                        if (mediaIds.Any())
                        {
                            var random = new Random();
                            var minIndex = mediaIds.Min();
                            var maxIndex = mediaIds.Max();
                            ids.AddRange(mediaIds
                                .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex)))
                                .Take(maxRandomMediaItems).OrderBy(i => i).ToList());
                        }
                    }
                }
            }
            else
            {
                var mediaIds = _context.Media.Where(x => x.MediastatusId == (int)MediaStatusEnum.Published &&
                                         !x.IsDeleted && !x.IsPrivate &&
                                        (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                          x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                         x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc))
                                          .Select(y => y.Id).ToList();

                if (mediaIds.Any())
                {
                    var random = new Random();
                    var minIndex = mediaIds.Min();
                    var maxIndex = mediaIds.Max();
                    ids.AddRange(mediaIds
                        .OrderBy(i => minIndex + (long)(random.NextDouble() * (maxIndex - minIndex)))
                        .Take(maxRandomMediaItems).OrderBy(i => i).ToList());
                }
            }

            var result = ( from media in _context.Media.Include(a => a.Series).ThenInclude(b => b.Seriestype).Include(y => y.ToolMedia).AsEnumerable().Where(x => ids.Contains(x.Id)).ToList()
                           let topic = ( from mtp in _context.MediaTopic.ToList()
                                         join tp in _context.Topic on mtp.TopicId equals tp.Id into tpGroup
                                         from topic in tpGroup.DefaultIfEmpty()
                                         where mtp.MediaId == media.Id
                                         select topic.Name ).FirstOrDefault()
                           let resource = ( from tm in media.ToolMedia
                                            join tool in _context.Tool on tm.ToolId equals tool.Id
                                            where tm.MediaId == media.Id
                                            orderby tool.Id select new { tool.Name, tool.Url} ).FirstOrDefault()
                           select new OptimisedVideosResponse
                           {
                               Id = media.Id ,
                               Title = media.Name ,
                               Description = media.Description ,
                               FeaturedImage = media.FeaturedImage ,
                               SeoUrl = media.SeoUrl ,
                               SeriesType = media.Series?.Seriestype?.Name ,
                               Series = media.Series?.Name ,
                               Topic = topic ,
                               Url = media.Url ,
                               MediaTypeId = media.MediatypeId ,
                               UniqueId = media.UniqueId ,
                               MediaMetadata = media.Metadata,
                               ResourceTitle = resource != null ? resource.Name : null,
                               ResourceUrl = resource != null ? resource.Url : null
                           } ).OrderBy(d => ids.IndexOf(d.Id)).ToList();

            await Task.Run(() =>
            {
                Parallel.ForEach(result , async media =>
                {
                    if ( media.MediaMetadata != null )
                    {
                        var metadata = JsonConvert.DeserializeObject<MediaMetaData>(media.MediaMetadata);
                        media.MediaDuration = metadata.duration;
                    }
                    media.FeaturedImage =  ( await _s3BucketService.GetCompressedImages(media.FeaturedImage , EntityType.Media) ).Banner;
                });
            });

            if (result.Count > 0)
            {
                return result;
            }
            return new List<OptimisedVideosResponse>();
        }

        public async Task<CollectionModel<ToolViewModel>> GetToolsByTopicIdAsync(int topicId, int pageNumber, int pageSize)
        {
            var toolIds = _context.ToolTopic.Where(x => x.TopicId == topicId).Select(y => y.ToolId).ToList();
            var query = _context.Tool.Where(tool => toolIds.Contains(tool.Id)).Select(
                        tool => new ToolViewModel
                        {
                            Id = tool.Id,
                            Name = tool.Name,
                            Description = tool.Description,
                            FeaturedImage = tool.FeaturedImage,
                            Link = tool.Url,
                            ShowOnMenu = (bool)tool.ShowOnMenu,
                            ShowOnHomePage = Convert.ToBoolean(tool.ShowOnHomepage),
                            PartnerId = tool.PartnerId,
                            DateCreated = tool.DateCreatedUtc
                        }).OrderByDescending(x => x.DateCreated);

            var totalCount = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            foreach (var item in result)
            {
                var featuredImage = item.FeaturedImage;
                item.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                item.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Tools);
            }
            return new CollectionModel<ToolViewModel>
            {
                Items = result,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<UserFacingSearchViewModel> GetUserFacingCloudSearchAsync(string filterString, int pageSize, int pageNumber, bool isIncludeEmbedded)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return new UserFacingSearchViewModel();
            }
            var searchOutput = new UserFacingSearchViewModel
            {
                TopResults = new TopResult()
            };

            var totalTopResult = 0;

            var blogItems = await GetBlogBySearchText(filterString);
            var _toolitems = _cloudTopicToolSeriesProvider.ToolCloudSearch(filterString, 1, 1, Constant._defaultsortOrder, "", out var toolCount);
            var _topictems = _cloudTopicToolSeriesProvider.TopicCloudSearch(filterString, 1, 1, Constant._defaultsortOrder, "", out var topicCount);
            var _seriesItems = _cloudTopicToolSeriesProvider.SeriesCloudSearch(filterString, 1, 1, Constant._defaultsortOrder, "", out var seriesCount);
            var fields = _mediaCloudSearch.SearchFromCloud(filterString, pageSize, pageNumber, isIncludeEmbedded, Constant._defaultsortOrder, "", out var countm);

            var blogs = blogItems.Select(x => new Blog
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                FeaturedImage = x.FeaturedImage,
                Url = x.Url,
                PublishedDate = x.PublishedDate
            }).ToList();

            var toolItems = new CloudSearch.Service.Model.Tool();
            var toolItem = _toolitems.FirstOrDefault();
            if (toolItem != null)
            {
                totalTopResult++;
                toolItems.Description = toolItem.description;
                toolItems.Id = Convert.ToInt32(toolItem.id.Replace("Tool", ""));
                toolItems.Name = toolItem.title;
                toolItems.Url = toolItem.link;
                toolItems.ShowOnMenu = Convert.ToBoolean(toolItem.showonmenu);
                toolItems.ShowOnHomePage = Convert.ToBoolean(toolItem.showonhomepage);
                searchOutput.TopResults.Tool = toolItems;
                var featuredImage = toolItem.featuredimage;
                toolItems.FeaturedImage = !string.IsNullOrEmpty(toolItem.featuredimage) ? _s3BucketService.RetrieveImageCDNUrl(toolItem.featuredimage) : string.Empty;
                var featuredImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Tools);
                toolItems.FeaturedImages = new CloudSearch.Services.Images
                {
                    Banner = featuredImages.Banner,
                    Grid = featuredImages.Grid,
                    Thumbnail = featuredImages.Thumbnail,
                    SmallThumbnail = featuredImages.SmallThumbnail
                };
            }

            //topic

            var topicItems = new CloudSearch.Service.Model.Topic();
            var topicItem = _topictems.FirstOrDefault();
            if (topicItem != null)
            {
                totalTopResult++;
                topicItems.Description = topicItem.description;
                topicItems.Id = Convert.ToInt32(topicItem.id.Replace("Topic", ""));
                topicItems.Name = topicItem.title;
                topicItems.SeoUrl = topicItem.seourl;
                var logo = topicItem.logo;
                topicItems.FeaturedImage = !string.IsNullOrEmpty(topicItem.logo) ? _s3BucketService.RetrieveImageCDNUrl(topicItem.logo) : string.Empty;
                var featuredImages = await _s3BucketService.GetCompressedImages(logo, EntityType.Topics);
                topicItems.FeaturedImages = new CloudSearch.Services.Images
                {
                    Banner = featuredImages.Banner,
                    Grid = featuredImages.Grid,
                    Thumbnail = featuredImages.Thumbnail,
                    SmallThumbnail = featuredImages.SmallThumbnail
                };
                searchOutput.TopResults.Topic = topicItems;
            }

            //series

            var seriesItems = new CloudSearch.Service.Model.Series();
            var seriesItem = _seriesItems.FirstOrDefault();
            if (seriesItem != null)
            {
                totalTopResult++;
                seriesItems.Description = seriesItem.description;
                seriesItems.Id = Convert.ToInt32(seriesItem.id.Replace("Series", ""));
                seriesItems.SeoUrl = seriesItem.seourl;
                seriesItems.Name = seriesItem.title;
                var featuredImage = seriesItem.featuredimage;
                seriesItems.FeaturedImage = !string.IsNullOrEmpty(seriesItem.featuredimage) ? _s3BucketService.RetrieveImageCDNUrl(seriesItem.featuredimage) : string.Empty;
                var featuredImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Series);
                seriesItems.FeaturedImages = new CloudSearch.Services.Images
                {
                    Banner = featuredImages.Banner,
                    Grid = featuredImages.Grid,
                    Thumbnail = featuredImages.Thumbnail,
                    SmallThumbnail = featuredImages.SmallThumbnail
                };
                searchOutput.TopResults.Series = seriesItems;
            }

            //Media
            var medias = fields.Select(x => new CloudSearch.Service.Model.Media
            {
                Id = Convert.ToInt32(x.id),
                Name = x.title,
                FeaturedImage = x.logo,
                MediaType = x.mediatype,
                Series = x.seriestitle,
                Topic = x.topictitle != null ? x.topictitle.FirstOrDefault() : string.Empty,
                Description = x.description,
                IsSharingAllowed = Convert.ToBoolean(x.issharingallowed),
                Thumbnail = x.thumbnail,
                SeoUrl = x.seourl,
                UniqueId = x.uniqueid
            }).ToList();

            foreach (var media in medias)
            {
                var featuredImage = media.FeaturedImage;
                media.FeaturedImage = !string.IsNullOrEmpty(media.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(media.FeaturedImage) : string.Empty;
                var featuredImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Media);
                media.FeaturedImages = new CloudSearch.Services.Images
                {
                    Banner = featuredImages.Banner,
                    Grid = featuredImages.Grid,
                    Thumbnail = featuredImages.Thumbnail,
                    SmallThumbnail = featuredImages.SmallThumbnail
                };
                var thumbnail = media.Thumbnail;
                media.Thumbnail = !string.IsNullOrEmpty(media.Thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(media.Thumbnail) : string.Empty;
                var thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Media);
                media.Thumbnails = new CloudSearch.Services.Images
                {
                    Banner = thumbnails.Banner,
                    Grid = thumbnails.Grid,
                    Thumbnail = thumbnails.Thumbnail,
                    SmallThumbnail = thumbnails.SmallThumbnail
                };
                media.MediaTypeId = _context.MediaType.SingleOrDefault(s => s.Name.Trim() == media.MediaType.Trim()).Id;
            }

            searchOutput.Media = medias;
            return new UserFacingSearchViewModel
            {
                TopResults = searchOutput.TopResults,
                Media = searchOutput.Media,
                Blog = blogs,
                TotalMediaCount = countm,
                TotalBlogCount = blogs.Count,
                TotalTopCount = totalTopResult
            };
        }

        public async Task<List<FilteredResourceModel>> ResourcesStatisticsAsync()
        {
            var tools = _cloudTopicToolSeriesProvider.FilteredResourceCloudSearch().ToList();
            var result = (from item in tools
                          select new FilteredResourceModel
                          {
                              Type = item.Type,
                              Data = (from d in item.Data
                                      select new EntityModel
                                      {
                                          Name = d.value,
                                          Count = d.count
                                      }).ToList()
                          }
                         ).ToList();
            if (result.Count > 0)
            {
                return result;
            }

            return new List<FilteredResourceModel>();
        }

        public async Task<SeriesMediaModel> SeriesMediaDetailsAsync(int seriesId, int pageNumber, int pageSize, bool isIncludeEmbedded)
        {
            var seriesDetails = await _context.Series.Include(media => media.Media).Include(serieMedia => serieMedia.SeriesMedia).SingleOrDefaultAsync(series => series.Id == seriesId);
            if (seriesDetails is null)
            {
                throw new BusinessException("Series does not exist");
            }

            var seriesMedia = seriesDetails.Media.Where(media => media.IsPrivate == false && !media.IsDeleted && media.MediastatusId == (int)MediaStatusEnum.Published
                && media.MediatypeId != (int)MediaTypeEnum.Banner
                && (media.ActiveFromUtc.HasValue && media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc :
                                          media.ActiveFromUtc.HasValue && !media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow :
                                         media.ActiveFromUtc.HasValue || !media.ActiveToUtc.HasValue || DateTime.UtcNow <= media.ActiveToUtc));

            if (!isIncludeEmbedded)
            {
                seriesMedia = seriesMedia.Where(x => x.MediatypeId != (int)MediaTypeEnum.EmbeddedMedia);
            }

            var lstActiveMedia = new List<DataAccess.Entities.Media>();
            foreach (var curItem in seriesMedia)
            {
                if (curItem.ActiveFromUtc > curItem.DatePublishedUtc)
                {
                    curItem.DateCreatedUtc = Convert.ToDateTime(curItem.ActiveFromUtc);
                }
                else
                {
                    curItem.DateCreatedUtc = Convert.ToDateTime(curItem.DatePublishedUtc);
                }
                lstActiveMedia.Add(curItem);
            }

            lstActiveMedia = lstActiveMedia.OrderByDescending(media => media.DateCreatedUtc).ToList();

            var combinedActiveMedia = (from media in lstActiveMedia
                                       join sr in _context.Series.Include(x => x.Seriestype) on media.SeriesId equals sr.Id into srGroup
                                       from series in srGroup.DefaultIfEmpty()
                                       let topic = (from mtp in _context.MediaTopic
                                                    join tp in _context.Topic on mtp.TopicId equals tp.Id into tpGroup
                                                    from topic in tpGroup.DefaultIfEmpty()
                                                    where mtp.MediaId == media.Id
                                                    select topic.Name).FirstOrDefault()
                                       select new MediaInfoModel
                                       {
                                           Id = media.Id,
                                           Title = media.Name,
                                           Description = media.Description,
                                           Url = media.Url,
                                           Thumbnail = media.Thumbnail,
                                           FeaturedImage = media.FeaturedImage,
                                           IsSharingAllowed = media.IsSharingAllowed,
                                           MediaTypeId = media.MediatypeId,
                                           SeoUrl = media.SeoUrl,
                                           UniqueId = media.UniqueId,
                                           Series = series.Name,
                                           Topic = topic
                                       }).ToList();

            var totalMedias = combinedActiveMedia.Count;
            combinedActiveMedia = combinedActiveMedia.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var result = new SeriesMediaModel
            {
                SeriesId = seriesDetails.Id,
                Title = seriesDetails.Name,
                Description = seriesDetails.Description,
                SeoUrl = seriesDetails.SeoUrl,
                Logo = !string.IsNullOrEmpty(seriesDetails.Logo) ? _s3BucketService.RetrieveImageCDNUrl(seriesDetails.Logo) : string.Empty,
                Logos = await _s3BucketService.GetCompressedImages(seriesDetails.Logo, EntityType.Series),
                FeaturedImage = !string.IsNullOrEmpty(seriesDetails.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(seriesDetails.FeaturedImage) : string.Empty,
                FeaturedImages = await _s3BucketService.GetCompressedImages(seriesDetails.FeaturedImage, EntityType.Series),
                VideoCount = totalMedias,
                Videos = combinedActiveMedia,
                LogoSize = seriesDetails.LogoSize,
                DescriptionColor = seriesDetails.DescriptionColor
            };
            if (result != null)
            {
                foreach (var media in result.Videos)
                {
                    var thumbnail = media.Thumbnail;
                    media.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                    media.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Media);

                    var logo = media.Logo;
                    media.Logo = !string.IsNullOrEmpty(logo) ? _s3BucketService.RetrieveImageCDNUrl(logo) : string.Empty;
                    media.Logos = await _s3BucketService.GetCompressedImages(logo, EntityType.Media);

                    var featuredImage = media.FeaturedImage;
                    media.FeaturedImage = !string.IsNullOrEmpty(media.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(media.FeaturedImage) : string.Empty;
                    media.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Media);
                }
                return result;
            }
            return null;
        }

        public async Task<TopicMediaModel> TopicMediaDetailsAsync(int topicId, int pageNumber, int pageSize, bool isIncludeEmbedded)
        {
            var topicDetails = await _context.Topic.SingleOrDefaultAsync(y => y.Id == topicId);

            if (topicDetails is null)
            {
                throw new BusinessException("Topic not exist");
            }

            var mediaQuery =  (from media in _context.Media.Include(x => x.Series).Where(y => y.MediastatusId == (int)MediaStatusEnum.Published &&
                                 y.MediatypeId != (int)MediaTypeEnum.Banner && !y.IsDeleted && y.IsPrivate == false
                                 && (y.ActiveFromUtc.HasValue && y.ActiveToUtc.HasValue ? y.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= y.ActiveToUtc :
                                         y.ActiveFromUtc.HasValue && !y.ActiveToUtc.HasValue ? y.ActiveFromUtc <= DateTime.UtcNow :
                                        y.ActiveFromUtc.HasValue || !y.ActiveToUtc.HasValue || DateTime.UtcNow <= y.ActiveToUtc))
                                join mediaTopic in _context.MediaTopic.Where(t => t.TopicId == topicId)
                                on media.Id equals mediaTopic.MediaId
                                select new MediaInfoModel
                                {
                                    Id = media.Id,
                                    Title = media.Name,
                                    Description = media.Description,
                                    Logo = media.FeaturedImage,
                                    Thumbnail = media.Thumbnail,
                                    Url = media.Url,
                                    DatePublishedUtc = media.DatePublishedUtc,
                                    MediaTypeId = media.MediatypeId,
                                    IsSharingAllowed = media.IsSharingAllowed,
                                    SeoUrl = media.SeoUrl,
                                    UniqueId = media.UniqueId,
                                    Series = media.Series.Name,
                                    Topic = topicDetails.Name,
                                }).OrderByDescending(x => x.DatePublishedUtc).AsQueryable();

            if (!isIncludeEmbedded)
            {
                mediaQuery = mediaQuery.Where(x => x.MediaTypeId != (int)MediaTypeEnum.EmbeddedMedia);
            }

            var totalMedias = await mediaQuery.CountAsync();
            var medias  = await mediaQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            foreach (var media in medias)
            {
                var thumbnail = media.Thumbnail;
                media.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                media.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Media);
                var logo = media.Logo;
                media.Logo = !string.IsNullOrEmpty(logo) ? _s3BucketService.RetrieveImageCDNUrl(logo) : string.Empty;
                media.Logos = await _s3BucketService.GetCompressedImages(logo, EntityType.Media);
            }
            var result = new TopicMediaModel
            {
                TopicId = topicDetails.Id,
                Title = topicDetails.Name,
                Description = topicDetails.Description,
                SeoUrl = topicDetails.SeoUrl,
                Logo = !string.IsNullOrEmpty(topicDetails.Logo) ? _s3BucketService.RetrieveImageCDNUrl(topicDetails.Logo) : string.Empty,
                Logos = await _s3BucketService.GetCompressedImages(topicDetails.Logo, EntityType.Topics),
                VideoCount = totalMedias,
                Videos = medias
            };
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public async Task<CollectionModel<MediaInfoModel>> UpcomingMediasAsync(long mediaId, int pageNumber, int pageSize, int itemType)
        {
            var mediaDetail = await _context.Media.Include(media => media.MediaTopic).SingleOrDefaultAsync(media => media.Id == mediaId);
            if (mediaDetail is null)
            {
                throw new BusinessException("Media does not exist");
            }

            var medias = new List<MediaInfoModel>();

            if (itemType == (int)ItemTypeEnum.Series)
            {
                if (mediaDetail.SeriesId != null)
                {
                    medias = await GetSeriesMedia(mediaDetail.SeriesId, mediaId);
                    if (medias.Count < pageSize)
                    {
                        if (mediaDetail.MediaTopic.Count > 0)
                        {
                            medias.AddRange(await GetTopicMedia(mediaId));
                            medias = medias.GroupBy(p => p.Id).Select(grp => grp.FirstOrDefault()).ToList();
                            if (medias.Count < pageSize)
                            {
                                medias.AddRange(await GetAllMedia(mediaId, itemType: (int)ItemTypeEnum.Both));
                                medias = medias.GroupBy(p => p.Id).Select(grp => grp.FirstOrDefault()).ToList();
                            }
                        }
                    }
                }
            }
            else if (itemType == (int)ItemTypeEnum.Topic)
            {
                medias = await GetTopicMedia(mediaId);
                if (medias.Count < pageSize)
                {
                    if (mediaDetail.SeriesId != null)
                    {
                        medias.AddRange(await GetSeriesMedia(mediaDetail.SeriesId, mediaId));
                        medias = medias.GroupBy(p => p.Id).Select(grp => grp.FirstOrDefault()).ToList();
                        if (medias.Count < pageSize)
                        {
                            medias.AddRange(await GetAllMedia(mediaId, itemType: (int)ItemTypeEnum.Both));
                            medias = medias.GroupBy(p => p.Id).Select(grp => grp.FirstOrDefault()).ToList();
                        }
                    }
                }
            }
            else
            {
                medias = await GetAllMedia(mediaId);
            }
            if (itemType > 0)
            {
                var restMedias = await GetAllMedia(mediaId, mediaDetail.SeriesId, itemType);
                medias.AddRange(restMedias);
                medias = medias.GroupBy(p => p.Id).Select(grp => grp.FirstOrDefault()).ToList();
            }
            // var totalMedias = medias.Count;
            medias = medias.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            foreach (var media in medias)
            {
                if (media.SeriesId != null)
                {
                    media.Series = await _context.Series.Where(x => x.Id == media.SeriesId).Select(y => y.Name).FirstOrDefaultAsync();
                }
                var thumbnail = media.Thumbnail;
                media.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                media.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Media);
                var featuredImage = media.FeaturedImage;
                media.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                media.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Media);
            }

            if (medias.Count > 0)
            {
                return new CollectionModel<MediaInfoModel> { Items = medias };
            }

            return new CollectionModel<MediaInfoModel>();
        }

        #region Private Methods
        private bool IsMediaActive(long mediaId)
        {
            var media = _context.Media.FirstOrDefault(x => x.Id == mediaId);

            if (media != null)
            {
                if (media.ActiveFromUtc != null && media.ActiveToUtc != null)
                    return media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc;
                else if (media.ActiveFromUtc != null && media.ActiveToUtc == null)
                    return media.ActiveFromUtc <= DateTime.UtcNow;
                else if (media.ActiveFromUtc == null && media.ActiveToUtc != null)
                    return DateTime.UtcNow <= media.ActiveToUtc;
                else
                    return true;
            }
            return false;
        }

        private async Task<List<MediaInfoModel>> GetSeriesMedia(int? seriesId, long mediaId)
        {
            var medias = new List<MediaInfoModel>();
            medias = await _context.Media.Where(media => media.SeriesId == seriesId && !media.IsDeleted && media.IsPrivate == false
                    && media.MediastatusId == (int)MediaStatusEnum.Published
                    && media.MediatypeId != (int)MediaTypeEnum.Banner
                    && media.MediatypeId != (int)MediaTypeEnum.EmbeddedMedia
                    && media.Id != mediaId
                    && (media.ActiveFromUtc.HasValue && media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc :
                         media.ActiveFromUtc.HasValue && !media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow :
                         media.ActiveFromUtc.HasValue || !media.ActiveToUtc.HasValue || DateTime.UtcNow <= media.ActiveToUtc))
                 .OrderByDescending(media => media.DatePublishedUtc)
                 .Select(item =>
                InitializeItems(item)).ToListAsync();
            return medias;
        }

        private async Task<List<MediaInfoModel>> GetTopicMedia(long mediaId)
        {
            var medias = new List<MediaInfoModel>();
            var topics = await _context.MediaTopic.Where(mTopic => mTopic.MediaId == mediaId).Select(mTopic => mTopic.TopicId).ToListAsync();
            var mediaIds = await _context.MediaTopic.Where(mTopic => topics.Contains(mTopic.TopicId))
                                .Select(mTopic => mTopic.MediaId).Distinct().ToListAsync();

            medias = await _context.Media.Where(media => mediaIds.Contains(media.Id) && !media.IsDeleted && media.IsPrivate == false
              && media.MediastatusId == (int)MediaStatusEnum.Published
              && media.MediatypeId != (int)MediaTypeEnum.Banner
              && media.MediatypeId != (int)MediaTypeEnum.EmbeddedMedia
              && media.Id != mediaId
              && (media.ActiveFromUtc.HasValue && media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc :
                   media.ActiveFromUtc.HasValue && !media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow :
                   media.ActiveFromUtc.HasValue || !media.ActiveToUtc.HasValue || DateTime.UtcNow <= media.ActiveToUtc))
                 .OrderByDescending(media => media.DatePublishedUtc)
                .Select(item =>
          InitializeItems(item)).ToListAsync();
            return medias;
        }

        private async Task<List<MediaInfoModel>> GetAllMedia(long mediaId, int? seriesId = 0, int itemType = 0)
        {
            var medias = new List<MediaInfoModel>();
            switch (itemType)
            {
                case (int)ItemTypeEnum.Series:

                    medias = await _context.Media.Where(media => media.IsPrivate == false
                        && !media.IsDeleted
                        && media.MediastatusId == (int)MediaStatusEnum.Published
                        && media.MediatypeId != (int)MediaTypeEnum.Banner
                        && media.MediatypeId != (int)MediaTypeEnum.EmbeddedMedia
                        && media.SeriesId != seriesId
                        && media.Id != mediaId
                        && (media.ActiveFromUtc.HasValue && media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc :
                         media.ActiveFromUtc.HasValue && !media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow :
                         media.ActiveFromUtc.HasValue || !media.ActiveToUtc.HasValue || DateTime.UtcNow <= media.ActiveToUtc))
                          .OrderByDescending(media => media.DatePublishedUtc)
                         .Select(item =>
                   InitializeItems(item)).ToListAsync();
                    break;

                case (int)ItemTypeEnum.Topic:

                    var topics = await _context.MediaTopic.Where(mTopic => mTopic.MediaId == mediaId).Select(mTopic => mTopic.TopicId).ToListAsync();
                    var mediaIds = await _context.MediaTopic.Where(mTopic => topics.Contains(mTopic.TopicId))
                                        .Select(mTopic => mTopic.MediaId).Distinct().ToListAsync();
                    medias = await _context.Media.Where(media => media.IsPrivate == false
                        && !media.IsDeleted
                        && media.MediastatusId == (int)MediaStatusEnum.Published
                        && media.MediatypeId != (int)MediaTypeEnum.Banner
                        && media.MediatypeId != (int)MediaTypeEnum.EmbeddedMedia
                        && !mediaIds.Contains(media.Id)
                        && media.Id != mediaId
                        && (media.ActiveFromUtc.HasValue && media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc :
                         media.ActiveFromUtc.HasValue && !media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow :
                         media.ActiveFromUtc.HasValue || !media.ActiveToUtc.HasValue || DateTime.UtcNow <= media.ActiveToUtc))
                          .OrderByDescending(media => media.DatePublishedUtc)
                         .Select(item =>
                    InitializeItems(item)).ToListAsync();
                    break;

                case (int)ItemTypeEnum.Both:
                    var serieId = _context.Media.Where(x => x.Id == mediaId).Select(x => x.SeriesId).FirstOrDefault();
                    var seriesMediaIds = await _context.Media.Where(x => x.SeriesId == serieId).Select(x => x.Id).ToListAsync();
                    var topicIds = await _context.MediaTopic.Where(mTopic => mTopic.MediaId == mediaId).Select(mTopic => mTopic.TopicId).ToListAsync();
                    var topicMediaIds = await _context.MediaTopic.Where(mTopic => topicIds.Contains(mTopic.TopicId))
                                        .Select(mTopic => mTopic.MediaId).Distinct().ToListAsync();
                    seriesMediaIds.AddRange(topicMediaIds);
                    seriesMediaIds = seriesMediaIds.Distinct().ToList();
                    medias = await _context.Media.Where(media => media.IsPrivate == false
                        && !media.IsDeleted
                        && media.MediastatusId == (int)MediaStatusEnum.Published
                        && media.MediatypeId != (int)MediaTypeEnum.Banner
                        && media.MediatypeId != (int)MediaTypeEnum.EmbeddedMedia
                        && !seriesMediaIds.Contains(media.Id)
                        && media.Id != mediaId
                        && (media.ActiveFromUtc.HasValue && media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc :
                         media.ActiveFromUtc.HasValue && !media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow :
                         media.ActiveFromUtc.HasValue || !media.ActiveToUtc.HasValue || DateTime.UtcNow <= media.ActiveToUtc))
                          .OrderByDescending(media => media.DatePublishedUtc)
                         .Select(item =>
                    InitializeItems(item)).ToListAsync();
                    break;

                default:
                    medias = await _context.Media.Where(media => media.IsPrivate == false
                        && !media.IsDeleted
                        && media.MediastatusId == (int)MediaStatusEnum.Published
                        && media.MediatypeId != (int)MediaTypeEnum.Banner
                        && media.MediatypeId != (int)MediaTypeEnum.EmbeddedMedia
                        && media.Id != mediaId
                        && (media.ActiveFromUtc.HasValue && media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc :
                         media.ActiveFromUtc.HasValue && !media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow :
                         media.ActiveFromUtc.HasValue || !media.ActiveToUtc.HasValue || DateTime.UtcNow <= media.ActiveToUtc))
                          .OrderByDescending(media => media.DatePublishedUtc)
                         .Select(item =>
                   InitializeItems(item)).ToListAsync();
                    break;
            }
            return medias;
        }

        private static MediaInfoModel InitializeItems(DataAccess.Entities.Media item)
        {
            return new MediaInfoModel
            {
                Id = item.Id,
                Title = item.Name,
                Description = item.Description,
                Url = item.Url,
                Thumbnail = item.Thumbnail,
                FeaturedImage = item.FeaturedImage,
                IsSharingAllowed = item.IsSharingAllowed,
                MediaTypeId = item.MediatypeId,
                SeoUrl = item.SeoUrl,
                UniqueId = item.UniqueId,
                SeriesId = item.SeriesId
            };
        }

        private async Task<List<BlogResponse>> GetBlogBySearchText(string searchText)
        {
            var blogs = new List<BlogResponse>();
            try
            {
                var wordPress = new WordPressClient(_appSettings.KinstaUrl);
                var posts = await wordPress.Posts.Query(new PostsQueryBuilder
                {
                    Search = searchText,
                    //Page = pageNumber ,
                    //PerPage = pageSize ,
                    OrderBy = WordPressPCL.Models.PostsOrderBy.Date,
                    Statuses = new WordPressPCL.Models.Status[] { WordPressPCL.Models.Status.Publish }
                });

                var mediaIds = posts.Where(x => x.FeaturedMedia != null).Select(y => y.FeaturedMedia).ToArray();

                var filteredMedias = await wordPress.Media.Query(new MediaQueryBuilder
                {
                    Include = Array.ConvertAll(mediaIds, x => x ?? 0)
                });


                foreach (var post in posts)
                {
                    string imageUrl = null;
                    var media = filteredMedias.Where(x => x.Post == post.Id).FirstOrDefault();
                    if (media != null)
                    {
                        imageUrl = media.MediaDetails.Sizes["et-pb-image--responsive--phone"].SourceUrl;
                    }
                    var blog = new BlogResponse()
                    {
                        Id = post.Id.ToString(),
                        PublishedDate = post.Date,
                        Title = post.Title.Rendered,
                        Description = post.Excerpt.Rendered.Replace("<p>", string.Empty).Replace("</p>", string.Empty).Replace("[&hellip;]", " ... "),
                        Url = post.Link.Substring(post.Link.IndexOf("/money-smarts/")),
                        FeaturedImage = imageUrl,
                        FetchedAt = DateTime.UtcNow
                    };

                    blogs.Add(blog);
                }
                return blogs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in method {nameof(GetBlogBySearchText)}: Exception is: {ex.Message} and StackTrace is: {ex.StackTrace}");
                return blogs;
            }
        }

        #endregion
    }
}
