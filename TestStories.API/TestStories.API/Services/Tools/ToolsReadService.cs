using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;


namespace TestStories.API.Services
{
    public class ToolsReadService: IToolsReadService
    {
        private readonly TestStoriesContext _context;
        private readonly IS3BucketService _s3BucketService;
        public ToolsReadService(TestStoriesContext context, IS3BucketService s3BucketService)
        {
            _context = context;
            _s3BucketService = s3BucketService;
        }
        private IQueryable<Tool> Tools => _context.Tool;

        private async Task<List<ExportResourceModel>> FilteredResources (ExportResourceFilter filter )
        {
            var items = new List<ExportResourceModel>();
            var sortedProperty = filter.SortedProperty == null || filter.SortedProperty == "" ? "title" : Convert.ToString(filter.SortedProperty).ToLower();
            var sortOrder = filter.SortOrder == null || filter.SortOrder == "" ? "ascending" : Convert.ToString(filter.SortOrder).ToLower();

             items =   ( from x in _context.Tool.Include(y => y.Tooltype).Include(z => z.Partner).ToList()
                      let topicNames = ( from m in _context.ToolTopic.Where(t => t.ToolId == x.Id).ToList()
                                         join n in _context.Topic
                                         on m.TopicId equals n.Id
                                         select n.Name ).ToList()

                        let assingnedTo = (from m in _context.ToolMedia.Where(t => t.ToolId == x.Id).ToList()
                                           select m.MediaId.ToString()).ToList()
                                           .Union(from series in _context.ToolSeries.Where(t => t.ToolId == x.Id).ToList()
                                                   join seriesName in _context.Series
                                                   on series.SeriesId equals seriesName.Id
                                                   select seriesName.Name).ToList()

                         select new ExportResourceModel
                      {
                          ResourceId = x.Id ,
                          ResourceTitle = x.Name ,
                          Description = x.Description ,
                          Url = x.Url ,
                          FeaturedImage = x.FeaturedImage ,
                          DateCreated = x.DateCreatedUtc.ToString() ,
                          SelectedPartner = x.Partner?.Name ?? "" ,
                          AssignedTo = assingnedTo.Count > 0 ? string.Join(",",assingnedTo) : "",
                          Topics = topicNames.Count > 0 ? string.Join("," , topicNames) : "",
                          ResourceType = x.Tooltype?.Name ?? "",
                          ShowOnMenu = ( x.ShowOnMenu != null && x.ShowOnMenu == true ) ? "YES" : "NO" ,
                          ShowOnHomePage = x.ShowOnHomepage == true ? "YES" : "NO" ,
                      } ).OrderByDescending(x => x.DateCreated).ToList();

            if ( !string.IsNullOrEmpty(filter.FilterString) )
            {
                items = items.Where(tool => tool.ResourceTitle == filter.FilterString).ToList();
            }

            if ( !string.IsNullOrEmpty(Convert.ToString(sortedProperty)) && !string.IsNullOrEmpty(Convert.ToString(sortOrder)) )
            {
                if ( sortedProperty.ToLower() == "title" && sortOrder == "ascending" )
                {
                    items = items.OrderBy(x => x.ResourceTitle).ToList();
                }
                if ( Convert.ToString(sortedProperty).ToLower() == "title" && sortOrder == "descending" )
                {
                    items = items.OrderByDescending(x => x.ResourceTitle).ToList();
                }

                if ( sortedProperty.ToLower() == "datecreated" && sortOrder == "ascending" )
                {
                    items = items.OrderBy(x => x.DateCreated).ToList();
                }
                if ( Convert.ToString(sortedProperty).ToLower() == "datecreated" && sortOrder == "descending" )
                {
                    items = items.OrderByDescending(x => x.DateCreated).ToList();
                }
            }
            
            foreach ( var resource in items )
            {
                resource.FeaturedImage = !string.IsNullOrEmpty(resource.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(resource.FeaturedImage) : string.Empty;
            }
            return items;
        }

        public async Task<CollectionModel<ShortToolModel>> GetClientTools(FilterClientToolViewRequest filter)
        {
            var query = (from x in Tools.Include(y => y.Tooltype).Where(x => x.ShowOnMenu == true)
                         select new ShortToolModel
                         {
                             Id = x.Id,
                             ToolName = x.Name,
                             Description = x.Description,
                             Url = x.Url,
                             Type = x.Tooltype.Name,
                             DateCreated=x.DateCreatedUtc,
                             FeaturedImage = x.FeaturedImage,
                             ShowOnHomePage = x.ShowOnHomepage
                         }).OrderByDescending(x => x.DateCreated);

            var totalCount = await query.CountAsync();

         
            var tools = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            foreach ( var tool in tools )
            {
                var featuredImage = tool.FeaturedImage;
                tool.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                tool.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Tools);
            }

            return new CollectionModel<ShortToolModel>
            {
                Items = tools,
                TotalCount = totalCount
            };
        }
        public async Task<CollectionModel<ToolItemOutput>> GetResources(FilterClientToolViewRequest request)
        {
            var query = (from x in Tools
                        select new ToolItemOutput
                        {
                               Id = x.Id,
                               ToolName = x.Name,
                               Title = x.Name,
                               DateCreated = x.DateCreatedUtc,
                               Description = x.Description,
                               Url = x.Url,
                               FeaturedImage = x.FeaturedImage
                         })
                        .OrderByDescending(x => x.DateCreated) as IQueryable<ToolItemOutput>;


            var totalCount = await query.CountAsync();

            if(request.Page > 0)
            {
                query = query.Skip((request.Page - 1) * request.PageSize);
            }

            if (request.PageSize > 0)
            {
                query = query.Take(request.PageSize);
            }

            var tools = await query.ToListAsync();
            foreach (var tool in tools)
            {
                var featuredImage = tool.FeaturedImage;
                tool.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ?  _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                tool.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Tools);
            }

            var result = new CollectionModel<ToolItemOutput>
            {
                Items = tools,
                TotalCount = totalCount
            };

            return result;
        }

        public async Task<ToolViewModel> GetToolAsync (int id)
        {
            var query = _context.Tool.Include(a => a.ToolSeries).Include(b => b.ToolMedia).Include(c => c.ToolTopic).Select(x => new ToolViewModel
            {
                Id = x.Id ,
                Name = x.Name ,
                ToolTypeId = x.TooltypeId ,
                PartnerId = x.PartnerId ,
                Description = x.Description ,
                Link = x.Url ,
                LstSeries = x.ToolSeries.Join(_context.Series ,
                           toolSeries => toolSeries.SeriesId ,
                           series => series.Id ,
                           (toolSeries , series) => new { toolSeries , series })
                           .Where(y => y.toolSeries.ToolId == id).Select(z => new ShortSeriesModel { Id = z.toolSeries.SeriesId , SeriesTitle = z.series.Name }).ToList() ,
                Medias = x.ToolMedia.Join(_context.Media ,
                        toolMedia => toolMedia.MediaId ,
                        media => media.Id ,
                        (toolMedia , media) => new { toolMedia , media })
                       .Where(y => y.toolMedia.ToolId == id).Select(z => new MediaShortModel { Id = z.toolMedia.MediaId , Name = z.media.Name }).ToList() ,
                Topics = x.ToolTopic.Join(_context.Topic ,
                           toolTopic => toolTopic.TopicId ,
                           topic => topic.Id ,
                           (toolTopic , topic) => new { toolTopic , topic })
                           .Where(y => y.toolTopic.ToolId == id).Select(z => new ShortTopicModel { Id = z.toolTopic.TopicId , Name = z.topic.Name }).ToList() ,
                ShowOnMenu = Convert.ToBoolean(x.ShowOnMenu) ,
                ShowOnHomePage = Convert.ToBoolean(x.ShowOnHomepage) ,
                FeaturedImage = x.FeaturedImage ,
                FeaturedImageFileName = x.FeaturedImageMetadata

            }).Where(s => s.Id == id);

            var result = await query.SingleOrDefaultAsync();

            if ( result != null )
            {
                var featuredImage = result.FeaturedImage;
                result.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                result.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage , EntityType.Tools);
                return result;
            }
            return null;
        }

        public async Task<CollectionModel<ShortToolModel>> GetToolViewCollectionAsync (FilterToolViewRequest filter)
        {
            List<ShortToolModel> items;
            var itemCount = 0;
            var sortedProperty = filter.SortedProperty == null || filter.SortedProperty == "" ? "title" : Convert.ToString(filter.SortedProperty).ToLower();
            var sortOrder = filter.SortOrder == null || filter.SortOrder == "" ? "ascending" : Convert.ToString(filter.SortOrder).ToLower();

            var allDbItems = _context.Tool.ToList();
            items = ( from x in allDbItems
                      let mediaIds = ( from m in _context.ToolMedia.Where(t => t.ToolId == x.Id).ToList()
                                       select m.MediaId.ToString()
                                     ).ToList()
                      let seriesNames = ( from series in _context.ToolSeries.Where(t => t.ToolId == x.Id).ToList()
                                          join seriesName in _context.Series
                                          on series.SeriesId equals seriesName.Id
                                          select seriesName.Name ).ToList()

                      let topicNames = ( from m in _context.ToolTopic.Where(t => t.ToolId == x.Id).ToList()
                                         join n in _context.Topic
                                         on m.TopicId equals n.Id
                                         select n.Name ).ToList()

                      select new ShortToolModel
                      {
                          Id = Convert.ToInt32(x.Id) ,
                          ToolName = x.Name ,
                          Title = x.Name ,
                          DateCreated = x.DateCreatedUtc ,
                          Description = x.Description ,
                          Url = x.Url ,
                          AssignmentForCloud = mediaIds.Concat(seriesNames).ToList() ,
                          TopicNames = topicNames

                      } ).OrderByDescending(x => x.DateCreated).ToList();

            if ( !string.IsNullOrEmpty(filter.FilterString) )
            {
                items = items.Where(tool => tool.Title == filter.FilterString).ToList();
            }
            itemCount = items.Count;


            if ( !string.IsNullOrEmpty(Convert.ToString(sortedProperty)) && !string.IsNullOrEmpty(Convert.ToString(sortOrder)) )
            {
                if ( sortedProperty.ToLower() == "title" && sortOrder == "ascending" )
                {
                    items = items.OrderBy(x => x.Title).ToList();
                }
                if ( Convert.ToString(sortedProperty).ToLower() == "title" && sortOrder == "descending" )
                {
                    items = items.OrderByDescending(x => x.Title).ToList();
                }

                if ( sortedProperty.ToLower() == "datecreated" && sortOrder == "ascending" )
                {
                    items = items.OrderBy(x => x.DateCreated).ToList();
                }
                if ( Convert.ToString(sortedProperty).ToLower() == "datecreated" && sortOrder == "descending" )
                {
                    items = items.OrderByDescending(x => x.DateCreated).ToList();
                }

            }

            items = items.Skip(( filter.Page - 1 ) * filter.PageSize).Take(filter.PageSize).ToList();

            return new CollectionModel<ShortToolModel>
            {
                Items = items ,
                TotalCount = itemCount
            };
        }

        public async Task<CollectionModel<ToolAutocompleteModel>> ToolAutoCompleteSearch (bool showOnHomepage = false)
        {
            var tools = new List<ToolAutocompleteModel>();
            if ( showOnHomepage )
            {
                tools=  await(from tool in _context.Tool
                              where tool.ShowOnHomepage == true
                              select new ToolAutocompleteModel { Id = tool.Id , ToolName = tool.Name , Title = tool.Name, Link = tool.Url }).ToListAsync();
            }
            else
            {
                tools=  await(from tool in _context.Tool
                              select new ToolAutocompleteModel { Id = tool.Id , ToolName = tool.Name , Title = tool.Name, Link = tool.Url }).ToListAsync();
            }
            var allItems = new CollectionModel<ToolAutocompleteModel> { Items = tools , TotalCount = tools.Count };
            return allItems;
        }

        private async Task ExportJsonToExcel (List<ExportResourceModel> resources)
        {
            var book = new Workbook();
            var sheet = book.Worksheets[0];
            sheet.Cells.ImportCustomObjects((System.Collections.ICollection)resources ,
                new string[] { "ResourceId" , "ResourceTitle" , "Description" , "Url" , "Topics" , "AssignedTo" , "SelectedPartner" , "ResourceType" , "FeaturedImage" , "ShowOnMenu" , "ShowOnHomePage" , "DateCreated" } ,
                true , // isPropertyNameShown
                0 , // firstRow
                0 , // firstColumn
                resources.Count , // Number of objects to be exported
                true , // insertRows
                null , // dateFormatString
                false); // convertStringToNumber
                        // Save the Excel file
            var stream = book.SaveToStream();
            await _s3BucketService.UploadExcelFileAsync(stream , $"{EnvironmentVariables.Env}-resources");
        }

        public async Task ExportResource (ExportResourceFilter filter)
        {
            var result = await FilteredResources(filter);
            await ExportJsonToExcel(result);
        }

        public async Task<CollectionModel<FeaturedResourceModel>> GetFeaturedResourcesAsync()
        {
            var settingValue = await _context.Setting
                .Where(s => s.Name == SettingKeyEnum.FeaturedResourcesSettings.ToString())
                .Select(s => s.Value)
                .SingleOrDefaultAsync();


            var settings = !string.IsNullOrEmpty(settingValue) ? JsonConvert.DeserializeObject<FeaturedSeriesSettingsModel>(settingValue) : null;
            var ids = settings.SetByAdmin && settings.Ids != null ? settings.Ids.Take(6) : Enumerable.Empty<int>();

            IQueryable<Tool> query = null;
            if(ids.Any())
            {
                query = _context.Tool
                    .Include(t => t.Tooltype)
                    .Include(t => t.Partner)
                    .Include(t => t.ToolTopic)
                        .ThenInclude(tt => tt.Topic)
                    .Where(t => ids.Contains(t.Id));
            }
            else
            {
                query = _context.Tool
                    .Include(t => t.Tooltype)
                    .Include(t => t.Partner)
                    .Include(t => t.ToolTopic)
                        .ThenInclude(tt => tt.Topic)
                    .OrderBy(t => Guid.NewGuid())
                    .Take(6);
            }

            var tools = await query.Select(t => new FeaturedResourceModel
            {
                Id = t.Id,
                Name = t.Name,
                TypeId = t.Tooltype.Id,
                Type = t.Tooltype.Name,
                PartnerId = t.Partner.Id,
                Partner = t.Partner.Name,
                Description = t.Description,
                Url = t.Url,
                Thumbnail = t.FeaturedImage,
                ShowOnHomePage = t.ShowOnHomepage,
                Topics = t.ToolTopic.Select(tt => tt.Topic.Name).ToList()
            }).ToListAsync();

            foreach (var tool in tools)
            {
                var thumbnail = tool.Thumbnail;
                tool.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                tool.Thumbnails = _s3BucketService.GetThumbnailImages(thumbnail, EntityType.Tools);
            }

            if (tools.Count > 0)
                return new CollectionModel<FeaturedResourceModel> { Items = tools, TotalCount = tools.Count };

            return new CollectionModel<FeaturedResourceModel>();
        }
    }
}
