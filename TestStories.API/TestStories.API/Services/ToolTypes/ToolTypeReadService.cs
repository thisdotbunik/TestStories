using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public class ToolTypeReadService : IToolTypeReadService
    {
        private readonly TestStoriesContext _context;

        public ToolTypeReadService(TestStoriesContext ctx)
        {
            _context = ctx;
        }
        public async Task<CollectionModel<ToolTypeListModel>> GetActiveToolTypesAsync()
        {
            var items = await (from x in _context.ToolType
                               where x.IsActive == true
                               select new ToolTypeListModel
                               {
                                   Id = x.Id,
                                   Name = x.Name
                               }).OrderBy(x => x.Name).ToListAsync();

            return new CollectionModel<ToolTypeListModel> { Items = items, TotalCount = items.Count };
        }

        public async Task<CollectionModel<ToolTypeListModel>> GetToolTypesAsync(FilterToolViewRequest filter)
        {
            var itemCount = 0;
            var sortedProperty = filter.SortedProperty == null || filter.SortedProperty == "" ? "name" : Convert.ToString(filter.SortedProperty).ToLower();
            var sortOrder = filter.SortOrder == null || filter.SortOrder == "" ? "ascending" : Convert.ToString(filter.SortOrder).ToLower();
            var items = (from type in _context.ToolType
                         select new ToolTypeListModel
                         {
                             Id = type.Id,
                             Name = type.Name,
                             Status = Convert.ToBoolean(type.IsActive) ? "Active" : "Not Active",
                             CreatedDateUtc = type.DateCreatedUtc
                         }).OrderBy(x => x.Name).ToList();

            if (!string.IsNullOrEmpty(filter.FilterString))
            {
                items = items.Where(tool => tool.Name == filter.FilterString).ToList();
            }
            itemCount = items.Count;

            switch (sortedProperty.ToLower())
            {
                case "status":
                    items = sortOrder.ToLower() == "ascending"
                        ? items.OrderBy(x => x.Status).ToList()
                        : items.OrderByDescending(x => x.Status).ToList();
                    break;

                case "createddate":
                    items = sortOrder.ToLower() == "ascending"
                        ? items.OrderBy(x => x.CreatedDateUtc).ToList()
                        : items.OrderByDescending(x => x.CreatedDateUtc).ToList();
                    break;
                default:
                    items = sortOrder.ToLower() == "ascending"
                      ? items.OrderBy(x => x.Name).ToList()
                      : items.OrderByDescending(x => x.Name).ToList();
                    break;
            }

            items = items.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();
            return new CollectionModel<ToolTypeListModel>
            {
                Items = items,
                TotalCount = itemCount
            };
        }

        public async Task<CollectionModel<ToolTypeAutoComplete>> ToolTypeAutoCompleteSearch()
        {
            var items = from x in _context.ToolType
                        select new ToolTypeAutoComplete { Id = x.Id, Title = x.Name };

            return new CollectionModel<ToolTypeAutoComplete> { Items = items, TotalCount = items.Count() };
        }
    }
}
