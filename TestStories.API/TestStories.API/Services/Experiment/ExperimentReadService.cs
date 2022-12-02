using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public class ExperimentReadService : IExperimentReadService
    {
        private readonly TestStoriesContext _context;
        private readonly IS3BucketService _s3BucketService;
        private readonly ILogger<ExperimentReadService> _logger;
        public ExperimentReadService (TestStoriesContext context , IS3BucketService s3BucketService, ILogger<ExperimentReadService> logger)
        {
            _context = context;
            _s3BucketService = s3BucketService;
            _logger = logger;
        }

        public async Task<CollectionModel<ExperimentAutoComplete>> ExperiementAutoCompleteAsync ()
        {
            var result = ( from x in _context.Experiment
                           select new ExperimentAutoComplete
                           {
                               ExperimentName = x.Name ,
                           } ).ToList();
            return new CollectionModel<ExperimentAutoComplete>
            {
                Items = result ,
                TotalCount = result.Count
            };
        }

        public async Task<CollectionModel<ExperimentListModel>> FilterExperimentAsync (ExperimentFilterRequest experimentFilter)
        {
            var experiments = await _context.Experiment.ToListAsync();

            if ( !string.IsNullOrEmpty(experimentFilter.FilterString) )
            {
                experiments = experiments.Where(x => x.Name.ToLower().Contains(experimentFilter.FilterString.ToLower())).ToList();
            }

            var items = ( from exp in experiments
                          join expType in _context.ExperimentType on exp.ExperimenttypeId equals expType.Id
                          join expStatus in _context.ExperimentStatus on exp.ExperimentstatusId equals expStatus.Id
                          join user in _context.User on exp.CreatedUserId equals user.Id
                          select new ExperimentListModel
                          {
                              Id = exp.Id ,
                              Name = exp.Name ,
                              CreatedBy = user.Name ,
                              StartDate = exp.StartDateUtc ,
                              EndDate = exp.EndDateUtc ,
                              ExperimentType = expType.Name ,
                              ExperimentStatus = expStatus.Name ,
                              Goal = exp.Goal ,
                              HypothesisMediaId = exp.MediaId ,
                              VideoPlays = exp.VideoPlays ,
                              EngagementTypeId = exp.EngagementtypeId
                          } ).OrderBy(x => x.Name).ToList();

            foreach ( var exp in items )
            {
                var expMedia = ( from expmedia in _context.ExperimentMedia
                                 join media in _context.Media
                                 on expmedia.MediaId equals media.Id
                                 where expmedia.ExperimentId == exp.Id
                                 select new ExperimentMediaModel
                                 {
                                     MediaId = expmedia.MediaId ,
                                     CardImageUuid = expmedia.TitleImage ,
                                     Name = media.Name
                                 } ).ToList();

                var lstMedia = new List<ExperimentMediaModel>();
                foreach ( var item in expMedia )
                {
                    var cardImage = !string.IsNullOrEmpty(item.CardImageUuid) ? _s3BucketService.RetrieveImageCDNUrl(item.CardImageUuid) : string.Empty;
                    lstMedia.Add(new ExperimentMediaModel { MediaId = item.MediaId , CardImageUuid = item.CardImageUuid , TitleCardImage = cardImage , Name = item.Name });
                }
                exp.Medias = lstMedia;
            }

            var _count = items.Count;

            if ( !string.IsNullOrEmpty(Convert.ToString(experimentFilter.SortedProperty)) && !string.IsNullOrEmpty(Convert.ToString(experimentFilter.SortOrder)) )
            {

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "name" && experimentFilter.SortOrder.ToString().ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.Name).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "name" && experimentFilter.SortOrder.ToString().ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.Name).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "createdby" && experimentFilter.SortOrder.ToString().ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.CreatedBy).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "createdby" && experimentFilter.SortOrder.ToString().ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.CreatedBy).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "startdate" && experimentFilter.SortOrder.ToString().ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.StartDate).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "startdate" && experimentFilter.SortOrder.ToString().ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.StartDate).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "enddate" && experimentFilter.SortOrder.ToString().ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.EndDate).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "enddate" && experimentFilter.SortOrder.ToString().ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.EndDate).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "experimenttype" && experimentFilter.SortOrder.ToString().ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.ExperimentType).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "experimenttype" && experimentFilter.SortOrder.ToString().ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.ExperimentType).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "experimentstatus" && experimentFilter.SortOrder.ToString().ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.ExperimentStatus).ToList();
                }

                if ( experimentFilter.SortedProperty.ToString().ToLower() == "experimentstatus" && experimentFilter.SortOrder.ToString().ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.ExperimentStatus).ToList();
                }
            }
            if ( experimentFilter.Page != 0 && experimentFilter.PageSize != 0 )
            {
                var skip = experimentFilter.PageSize * ( experimentFilter.Page - 1 );
                items = items.Skip(skip).Take(experimentFilter.PageSize).ToList();
            }

            return new CollectionModel<ExperimentListModel>
            {
                Items = items ,
                TotalCount = _count,
                PageNumber = experimentFilter.Page,
                PageSize = experimentFilter.PageSize
            };
        }

        public async Task<ExperimentViewModel> GetExperiementAsync (int id)
        {
            var result = await _context.Experiment.Select(x => new ExperimentViewModel
            {
                Id = x.Id ,
                Name = x.Name ,
                ExperimentTypeId = x.ExperimenttypeId ,
                ExperimentStatusId = x.ExperimentstatusId ,
                CreatedUserId = x.CreatedUserId ,
                StartDate = x.StartDateUtc ,
                EndDate = x.EndDateUtc ,
                Goal = x.Goal ,
                MediaId = x.MediaId ,
                VideoPlays = x.VideoPlays ,
                EngagementTypeId = x.EngagementtypeId ,
            }).Where(s => s.Id == id).SingleOrDefaultAsync();

            var expMedia = ( from exp in _context.ExperimentMedia
                             join media in _context.Media
                             on exp.MediaId equals media.Id
                             where exp.ExperimentId == id
                             select new
                             {
                                 exp.MediaId ,
                                 media.Name ,
                                 exp.TitleImage ,
                                 exp.VideoPlayCount ,
                                 exp.Shares ,
                                 exp.ToolClicks ,
                                 exp.Vw25 ,
                                 exp.Vw50 ,
                                 exp.Vw75 ,
                                 exp.Vw100
                             } ).ToList();

            _logger.LogError($"Get:Received request for ExperimentController and action:GetExperiementAsync");

            var lstMedia = new List<ExperimentMediaModel>();
            foreach ( var item in expMedia )
            {
                var cardImage = !string.IsNullOrEmpty(item.TitleImage) ? _s3BucketService.RetrieveImageCDNUrl(item.TitleImage) : string.Empty;
                lstMedia.Add(new ExperimentMediaModel
                {
                    MediaId = item.MediaId ,
                    MediaName = item.Name ,
                    CardImageUuid = item.TitleImage ,
                    TitleCardImage = cardImage ,
                    VideoWatch = item.VideoPlayCount ,
                    VideoShare = item.Shares ,
                    ToolClicks = item.ToolClicks ,
                    VW25 = item.Vw25 ,
                    VW50 = item.Vw50 ,
                    VW75 = item.Vw75 ,
                    VW100 = item.Vw100
                });
            }
            if ( lstMedia.Count > 0 )
            {
                result.LstMedia = lstMedia;
            }

            if ( result != null )
            {
                return result;
            }
            return null;
        }
    }
}
