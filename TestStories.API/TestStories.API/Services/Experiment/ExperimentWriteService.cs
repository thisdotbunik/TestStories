using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Services;
using TestStories.DataAccess.Enums;
using TestStories.API.Models.RequestModels;
using TestStories.DataAccess.Entities;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace TestStories.API.Concrete
{
    public class ExperimentWriteService : IExperimentWriteService
    {
        private readonly TestStoriesContext _context;
        private readonly IS3BucketService _s3BucketService;
        /// <inheritdoc />
        public ExperimentWriteService(TestStoriesContext context, IS3BucketService s3BucketService)
        {
            _context = context;
            _s3BucketService = s3BucketService;
        }
      
        private async Task<Experiment> AddExperiment(AddExperimentModel model, int userId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var experimentEntity = new Experiment
                    {
                        Name = model.Name,
                        ExperimenttypeId = model.ExperimentTypeId,
                        ExperimentstatusId = model.ExperimentStatusId,
                        StartDateUtc = model.StartDate,
                        EndDateUtc = model.EndDate,
                        CreatedUserId = userId,
                        Goal = model.Goal,
                        MediaId = model.MediaId,
                        VideoPlays = model.VideoPlays,
                        EngagementtypeId = model.EngagementTypeId.HasValue ? model.EngagementTypeId : null
                    };

                    await _context.AddAsync(experimentEntity);
                    await _context.SaveChangesAsync();

                    if (model.LstMedia != null)
                    {
                        var experimentMedia = new List<ExperimentMedia>();
                        foreach (var item in model.LstMedia)
                        {
                            experimentMedia.Add(new ExperimentMedia { ExperimentId = experimentEntity.Id, MediaId = item.MediaId, TitleImage = item.CardImageUuid });
                        }
                        _context.ExperimentMedia.AddRange(experimentMedia);
                        await _context.SaveChangesAsync();
                    }
                    transaction.Commit();
                    return experimentEntity;
                }
                catch
                {
                    transaction.Rollback();
                }
                return null;
            }
        }

        private async Task<Experiment> EditExperiment(int experimentId, EditExperimentModel model)
        {
            var dbExperiment = await _context.Experiment.SingleOrDefaultAsync(t => t.Id == experimentId);
            if (dbExperiment == null) return null;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    dbExperiment.Id = experimentId;
                    dbExperiment.Name = model.Name;
                    dbExperiment.ExperimentstatusId = model.ExperimentStatusId;
                    dbExperiment.StartDateUtc = model.StartDate;
                    dbExperiment.EndDateUtc = model.EndDate;
                    dbExperiment.Goal = model.Goal;
                    dbExperiment.MediaId = model.MediaId;
                    dbExperiment.VideoPlays = model.VideoPlays;
                    dbExperiment.EngagementtypeId = model.EngagementTypeId;
                    _context.Experiment.Update(dbExperiment);
                    await _context.SaveChangesAsync();

                    // TODO Used later when We will add remove experience Media at edit

                    //var lstOldMedia = await _context.ExperimentMedia.Where(x => x.ExperimentId == experimentId).ToListAsync();
                    //_context.RemoveRange(lstOldMedia);

                    //var lstExpMedia = new List<ExperimentMedia>();

                    //if (model.LstMedia != null)
                    //{
                    //    foreach (var item in model.LstMedia)
                    //    {
                    //        lstExpMedia.Add(new ExperimentMedia { ExperimentId = experimentId, MediaId = item.MediaId, TitleImage = item.CardImageUuid });
                    //    }
                    //}
                    //if (lstExpMedia.Count() > 0)
                    //{
                    //    _context.ExperimentMedia.AddRange(lstExpMedia);
                    //}
                    //await _context.SaveChangesAsync();
                    transaction.Commit();
                    return dbExperiment;
                }
                catch
                {
                    transaction.Rollback();
                }
                return null;
            }
        }

        private async Task<Experiment> UpdateExperimentStatus(Experiment entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task TrackEvent(int eventTypeId, long mediaId)
        {

            var experiments = (from exp in _context.Experiment
                               join expMedia in _context.ExperimentMedia
                               on exp.Id equals expMedia.ExperimentId
                               where exp.ExperimentstatusId == (int)ExperimentStatusEnum.Active && expMedia.MediaId == mediaId
                               select expMedia
                               ).ToList();

            // Increment the event counter for each Experiment

            switch (eventTypeId)
            {
                case (int)EventTypeEnum.VIDEO_WATCH:
                    foreach (var item in experiments)
                    {
                        item.VideoPlayCount += 1;
                    }
                    break;

                case (int)EventTypeEnum.VIDEO_SHARE:
                    foreach (var item in experiments)
                    {
                        item.Shares += 1;
                    }
                    break;

                case (int)EventTypeEnum.TOOL_CLICKS:
                    foreach (var item in experiments)
                    {
                        item.ToolClicks += 1;
                    }
                    break;
                case (int)EventTypeEnum.VW_25:
                    foreach (var item in experiments)
                    {
                        item.Vw25 += 1;
                    }
                    break;

                case (int)EventTypeEnum.VW_50:
                    foreach (var item in experiments)
                    {

                        item.Vw25 = item.Vw25 > 0 ? item.Vw25 - 1 : item.Vw25;
                        item.Vw50 += 1;
                    }
                    break;

                case (int)EventTypeEnum.VW_75:
                    foreach (var item in experiments)
                    {

                        item.Vw50 = item.Vw50 > 0 ? item.Vw50 - 1 : item.Vw50;
                        item.Vw75 += 1;
                    }
                    break;

                case (int)EventTypeEnum.VW_100:
                    foreach (var item in experiments)
                    {

                        item.Vw75 = item.Vw75 > 0 ? item.Vw75 - 1 : item.Vw75;
                        item.Vw100 += 1;
                    }
                    break;
            }

            _context.UpdateRange(experiments);
            await _context.SaveChangesAsync();
            if (eventTypeId == (int)EventTypeEnum.VIDEO_WATCH)
            {

                // Mark Experiment complete if EventType is VideoWatch.

                var lstActiveExperiment = (from expMedia in _context.ExperimentMedia
                                           join exp in _context.Experiment.Where(x => x.ExperimenttypeId == (int)ExperimentTypeEnum.EngagementDepth)
                                           on expMedia.ExperimentId equals exp.Id
                                           where exp.ExperimentstatusId == (int)ExperimentStatusEnum.Active
                                           group new { expMedia, exp } by new { expMedia.ExperimentId, exp.VideoPlays } into pg
                                           select new
                                           {
                                               experimentId = pg.Key.ExperimentId,
                                               thresholdPlayCount = pg.Key.VideoPlays,
                                               currentPlayCount = pg.Sum(x => x.expMedia.VideoPlayCount) // Get Sum of PlayCount for all the videos in the Experiment
                                           });

                var lstUpdatedExp = new List<Experiment>();
                foreach (var item in lstActiveExperiment)
                {
                    if (item.currentPlayCount == item.thresholdPlayCount)
                    {
                        var experiment = await _context.Experiment.SingleOrDefaultAsync(x => x.Id == item.experimentId);
                        if (experiment != null)
                        {
                            experiment.ExperimentstatusId = (int)ExperimentStatusEnum.Completed;
                            experiment.EndDateUtc = DateTime.UtcNow;
                        }
                        lstUpdatedExp.Add(experiment);
                    }
                }

                _context.Experiment.UpdateRange(lstUpdatedExp);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ExperimentViewModel> AddExperimentAsync (AddExperimentModel model, int userId)
        {
            if ( model.Name != null )
            {
                var isExperimentExist = _context.Experiment.Any(x => x.Name.Trim() == model.Name.Trim());
                if ( isExperimentExist )
                    throw new BusinessException("Name already exists, try another name");
            }

            if ( model.StartDate != null && model.EndDate != null )
            {
                if ( model.StartDate > model.EndDate )
                {
                    throw new BusinessException("start Date can not be greater than end Date");
                }
            }

            if ( model.LstMedia != null )
            {
                foreach ( var item in model.LstMedia )
                {
                    if ( item.TitleCardImage != null )
                    {
                        var currentImage = Base64ToImage(item.TitleCardImage.Split("base64,")[1] , item.Name);
                        var filePath = item.TitleCardImage != null ?
                        await _s3BucketService.UploadFileByTypeToStorageAsync(currentImage , item.MediaId , EntityType.None , FileTypeEnum.FeaturedImage.ToString()) : string.Empty;
                        item.CardImageUuid = filePath;
                    }
                }
            }
            var addExperiment = await AddExperiment(model , userId);
            return new ExperimentViewModel
            {
                Id = addExperiment.Id ,
                Name = addExperiment.Name ,
                CreatedUserId = addExperiment.CreatedUserId ,
                StartDate = addExperiment.StartDateUtc ,
                EndDate = addExperiment.EndDateUtc ,
                ExperimentStatusId = addExperiment.ExperimentstatusId ,
                ExperimentTypeId = addExperiment.ExperimenttypeId
            };
        }

        public async Task<ExperimentViewModel> EditExperimentAsync (int experimentId , EditExperimentModel model)
        {
            var dbExperiment = await _context.Experiment.SingleOrDefaultAsync(t => t.Id == experimentId);
            if ( dbExperiment == null )
            {
                throw new BusinessException("Experiment not found");
            }

            if ( model.Name != null )
            {
                var experimentDetail = await _context.Experiment.SingleOrDefaultAsync(x => x.Name == model.Name.Trim());
                if ( experimentDetail != null )
                    if ( experimentDetail.Id != experimentId )
                        throw new BusinessException("Name already exists, try another name");
            }

            if ( model.StartDate != null && model.EndDate != null )
            {
                if ( model.StartDate > model.EndDate )
                {
                    throw new BusinessException("start Date can not be greater than end Date");
                }
            }


            var experiment = await EditExperiment(experimentId , model);
            return new ExperimentViewModel
            {
                Id = experiment.Id ,
                Name = experiment.Name ,
                StartDate = experiment.StartDateUtc ,
                EndDate = experiment.EndDateUtc ,
                ExperimentStatusId = experiment.ExperimentstatusId ,
                ExperimentTypeId = experiment.ExperimenttypeId
            };
        }
        private IFormFile Base64ToImage (string base64String , string fileName)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            var ms = new MemoryStream(imageBytes , 0 , imageBytes.Length);
            var file = new FormFile(ms , 0 , ms.Length , null , fileName);
            return file;
        }

        public async Task<ExperimentViewModel> UpdateExperimentStatusAsync (UpdateExperimentModel model)
        {
            var experimentStatus = await _context.ExperimentStatus.FirstOrDefaultAsync(x => x.Id == model.StatusId);
            if ( experimentStatus == null )
            {
                throw new BusinessException("The incorrect experiement status");
            }

            var dbExperiment = await _context.Experiment.SingleOrDefaultAsync(x => x.Id == model.ExperimentId);
            if ( dbExperiment == null )
            {
                throw new BusinessException("Experiment not found");
            }
            Experiment experiment;
            if ( model.StatusId == (int)ExperimentStatusEnum.Completed )
            {
                dbExperiment.EndDateUtc = DateTime.UtcNow;
            }
            dbExperiment.ExperimentstatusId = model.StatusId;
            experiment = await UpdateExperimentStatus(dbExperiment);
            return new ExperimentViewModel
            {
                Id = experiment.Id ,
                Name = experiment.Name ,
                StartDate = experiment.StartDateUtc ,
                EndDate = experiment.EndDateUtc ,
                ExperimentStatusId = experiment.ExperimentstatusId ,
                ExperimentTypeId = experiment.ExperimenttypeId
            };
        }

    }
}
