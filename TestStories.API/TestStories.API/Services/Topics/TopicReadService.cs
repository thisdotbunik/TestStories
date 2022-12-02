using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Concrete
{
    public class TopicReadService : ITopicReadService
    {
        readonly TestStoriesContext _context;
        readonly IS3BucketService _s3BucketService;

        public TopicReadService (TestStoriesContext context , IS3BucketService s3BucketService)
        {
            _context = context;
            _s3BucketService = s3BucketService;
        }
        public async Task<CollectionModel<TopicModel>> GetTopicsAsync (int? parentId)
        {
            var query = _context.Topic.Include(t => t.Parent).Select(x => new TopicModel
            {
                Id = x.Id ,
                TopicName = x.Name ,
                Description = x.Description ,
                Logo = x.Logo ,
                ParentId = x.ParentId ,
                ParentTopic = x.ParentId.HasValue ? x.Parent.Name : string.Empty
            });

            if ( parentId.HasValue && parentId.Value > 0 )
                query = query.Where(t => t.ParentId == parentId);

            var result = await query.OrderBy(t => t.TopicName).ToListAsync();
            if ( result.Any() )
                return new CollectionModel<TopicModel> { Items = result , TotalCount = result.Count };

            return new CollectionModel<TopicModel>();
        }

        public async Task<CollectionModel<TopicViewModel>> GetTopicViewCollectionAsync (FilterTopicViewRequest filter)
        {
            var query =  _context.Topic.Include(x => x.Parent).Select(x => new TopicViewModel()
            {
                Id = x.Id,
                TopicName = x.Name,
                Title = x.Name,
                ParentTopic = x.Parent.Name,
                Description = x.Description,
                ParentId = x.ParentId != null ? Convert.ToInt32(x.ParentId) : 0,
                SeoUrl = x.SeoUrl
            });

            if ( !string.IsNullOrEmpty(filter.FilterString) )
            {
                query = query.Where(topic => topic.Title == filter.FilterString);
            }

            if ( !string.IsNullOrEmpty(Convert.ToString(filter.SortOrder)) && !string.IsNullOrEmpty(Convert.ToString(filter.SortedProperty)) )
            {
                // sorted by topic name
                if ( ( Convert.ToString(filter.SortOrder).ToLower().Trim() == "descending" ) && ( Convert.ToString(filter.SortedProperty).ToLower().Trim() == "title" ) )
                {
                    query = query.OrderByDescending(x => x.Title);
                }
                if ( ( Convert.ToString(filter.SortOrder).ToLower().Trim() == "ascending" ) && ( Convert.ToString(filter.SortedProperty).ToLower().Trim() == "title" ) )
                {
                    query = query.OrderBy(x => x.Title);
                }
                //sorted by parent topic
                if ( ( Convert.ToString(filter.SortOrder).ToLower().Trim() == "descending" ) && ( Convert.ToString(filter.SortedProperty).ToLower().Trim() == "parenttopic" ) )
                {
                    query = query.OrderByDescending(x => x.ParentTopic);
                }
                if ( ( Convert.ToString(filter.SortOrder).ToLower().Trim() == "ascending" ) && ( Convert.ToString(filter.SortedProperty).ToLower().Trim() == "parenttopic" ) )
                {
                    query = query.OrderBy(x => x.ParentTopic);
                }

                //sorted by description
                if ( ( Convert.ToString(filter.SortOrder).ToLower().Trim() == "descending" ) && ( Convert.ToString(filter.SortedProperty).ToLower().Trim() == "description" ) )
                {
                    query = query.OrderByDescending(x => x.Description);
                }
                if ( ( Convert.ToString(filter.SortOrder).ToLower().Trim() == "ascending" ) && ( Convert.ToString(filter.SortedProperty).ToLower().Trim() == "description" ) )
                {
                    query = query.OrderBy(x => x.Description);
                }
            }
            else
            {
                query = query.OrderBy(x => x.TopicName);
            }

            return new CollectionModel<TopicViewModel>
            {
                Items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync() ,
                TotalCount = await query.CountAsync(),
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<CollectionModel<TopicAutoCompleteModel>> TopicAutoCompleteSearch ()
        {
            var items = _context.Topic.Select(x => new TopicAutoCompleteModel
            {
                Name = x.Name ,
                Title = x.Name
            }).OrderBy(x => x.Name);

            return new CollectionModel<TopicAutoCompleteModel>
            {
                Items = items ,
                TotalCount = items.Count()
            };
        }

        public async Task<TopicModel> GetTopicAsync (int id)
        {
            var query = _context.Topic.Include(t => t.Parent).Where(t => t.Id == id);
            var result = await query.SingleOrDefaultAsync();
            if ( result != null )
            {
                var response = new TopicModel
                {
                    Id = result.Id ,
                    TopicName = result.Name ,
                    Description = result.Description ,
                    Logo = !string.IsNullOrEmpty(result.Logo) ? _s3BucketService.RetrieveImageCDNUrl(result.Logo) : string.Empty ,
                    Logos = await _s3BucketService.GetCompressedImages(result.Logo , EntityType.Topics) ,
                    ParentId = result.ParentId ,
                    ParentTopic = result.ParentId.HasValue ? result.Parent.Name : string.Empty ,
                    LogoFileName = result.LogoMetadata
                };
                return response;
            }
            return null;
        }

        public async Task<CollectionModel<ShortTopicModel>> GetShortTopicsAsync ()
        {
            var query = _context.Topic.Select(x => new ShortTopicModel { Id = x.Id , Name = x.Name }).OrderBy(t => t.Name);

            var result = await query.ToListAsync();
            if ( result.Any() )
                return new CollectionModel<ShortTopicModel> { Items = result , TotalCount = result.Count };

            return new CollectionModel<ShortTopicModel>();
        }
    }
}
