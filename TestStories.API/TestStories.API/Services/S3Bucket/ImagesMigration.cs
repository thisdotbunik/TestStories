using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestStories.Common;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;


namespace TestStories.API.Services
{
    public class ImageMigration: IImageMigration
    {
        readonly IS3BucketService _s3BucketService;
        readonly ILogger<ImageMigration> _logger;
        readonly IServiceProvider _services;

        public ImageMigration(IServiceProvider services, IS3BucketService s3BucketService, ILogger<ImageMigration> logger)
        {
            _services = services;
            _s3BucketService = s3BucketService;
            _logger = logger;
        }


        public async void ProcessImages(EntityType entityType)
        {
            try 
            {
                using (var scope = _services.CreateScope())
                {
                    using (var context = scope.ServiceProvider.GetRequiredService<TestStoriesContext>())
                    {
                        switch (entityType)
                        {
                            case EntityType.Media:
                                await ProcessMedia(context);
                                break;
                            case EntityType.Tools:
                                await ProcessTools(context);
                                break;
                            case EntityType.Topics:
                                await ProcessTopics(context);
                                break;
                            case EntityType.Series:
                                await ProcessSeries(context);
                                break;
                            default:
                                throw new NotImplementedException();
                        } 
                    }
                } 
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }


        public async Task ProcessMedia(TestStoriesContext context)
        {
            _logger.LogInformation("Processing media images...");
            PagedResult<Media> batch;
            int i = 1;

            do
            {
                batch = await context.Media.AsQueryable().GetBatch(i, 100);
                i++;

                foreach (var entity in batch.Results)
                {
                    if (!string.IsNullOrEmpty(entity.FeaturedImage))
                    {
                        if (await _s3BucketService.FileExists(entity.FeaturedImage))
                        {
                            var response = await _s3BucketService.GetTags(entity.FeaturedImage);
                            var tags = response.Tagging;
                            if (!tags.Any(tag => tag.Key == "EntityType") && !tags.Any(tag => tag.Key == "EntityId"))
                            {
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityType", Value = EntityType.Media.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityId", Value = entity.Id.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "FileType", Value = FileTypeEnum.FeaturedImage.ToString() });
                            }

                            await _s3BucketService.UpdateTags(entity.FeaturedImage, tags);
                           _logger.LogInformation("Processed media = " + entity.Id.ToString());
                            
                        }
                    }

                    if (!string.IsNullOrEmpty(entity.Thumbnail))
                    {
                        if (await _s3BucketService.FileExists(entity.Thumbnail))
                        {
                            var response = await _s3BucketService.GetTags(entity.Thumbnail);
                            var tags = response.Tagging;
                            if (!tags.Any(tag => tag.Key == "EntityType") && !tags.Any(tag => tag.Key == "EntityId"))
                            {
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityType", Value = EntityType.Media.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityId", Value = entity.Id.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "FileType", Value = FileTypeEnum.Thumbnail.ToString() });
                            }

                            await _s3BucketService.UpdateTags(entity.Thumbnail, tags);
                            _logger.LogInformation("Processed media = " + entity.Id.ToString());
                        }
                    }
                }

            } while (batch.Results.Count > 0);

            _logger.LogInformation("Completed media images. OK");
        }




        public async Task ProcessTools(TestStoriesContext context)
        {
            _logger.LogInformation("Processing tools images...");
            PagedResult<Tool> batch;
            int i = 1;

            do
            {
                batch = await context.Tool.AsQueryable().GetBatch(i, 100);
                i++;

                foreach (var entity in batch.Results)
                {
                    if (!string.IsNullOrEmpty(entity.FeaturedImage))
                    {
                        if (await _s3BucketService.FileExists(entity.FeaturedImage))
                        {
                            var response = await _s3BucketService.GetTags(entity.FeaturedImage);
                            var tags = response.Tagging;
                            if (!tags.Any(tag => tag.Key == "EntityType") && !tags.Any(tag => tag.Key == "EntityId"))
                            {
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityType", Value = EntityType.Tools.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityId", Value = entity.Id.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "FileType", Value = FileTypeEnum.FeaturedImage.ToString() });
                            }

                            await _s3BucketService.UpdateTags(entity.FeaturedImage, tags);
                           _logger.LogInformation("Processed tools = " + entity.Id.ToString());
                        }
                    }
                }

            } while (batch.Results.Count > 0);

            _logger.LogInformation("Completed tools images. OK");
        }


        public async Task ProcessTopics(TestStoriesContext context)
        {
            _logger.LogInformation("Processing topics images...");
            PagedResult<Topic> batch;
            int i = 1;

            do
            {
                batch = await context.Topic.AsQueryable().GetBatch(i, 100);
                i++;

                foreach (var entity in batch.Results)
                {
                    if (!string.IsNullOrEmpty(entity.Logo))
                    {
                        if (await _s3BucketService.FileExists(entity.Logo))
                        {
                            var response = await _s3BucketService.GetTags(entity.Logo);
                            var tags = response.Tagging;
                            if (!tags.Any(tag => tag.Key == "EntityType") && !tags.Any(tag => tag.Key == "EntityId"))
                            {
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityType", Value = EntityType.Topics.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityId", Value = entity.Id.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "FileType", Value = FileTypeEnum.Logo.ToString() });
                            }

                            await _s3BucketService.UpdateTags(entity.Logo, tags);
                            _logger.LogInformation("Processed topics = " + entity.Id.ToString());
                        }
                    }
                }

            } while (batch.Results.Count > 0);

            _logger.LogInformation("Completed topics images. OK");
        }



        public async Task ProcessSeries(TestStoriesContext context)
        {
            _logger.LogInformation("Processing series images...");

            PagedResult<Series> batch;
            int i = 1;

            do
            {
                batch = await context.Series.AsQueryable().GetBatch(i, 100);
                i++;

                foreach (var entity in batch.Results)
                {
                    if (!string.IsNullOrEmpty(entity.FeaturedImage))
                    {
                        if (await _s3BucketService.FileExists(entity.FeaturedImage))
                        {
                            var response = await _s3BucketService.GetTags(entity.FeaturedImage);
                            var tags = response.Tagging;
                            if (!tags.Any(tag => tag.Key == "EntityType") && !tags.Any(tag => tag.Key == "EntityId"))
                            {
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityType", Value = EntityType.Series.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityId", Value = entity.Id.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "FileType", Value = FileTypeEnum.FeaturedImage.ToString() });
                            }

                            await _s3BucketService.UpdateTags(entity.FeaturedImage, tags);
                            _logger.LogInformation("Processed series = " + entity.Id.ToString());
                        }
                    }

                    if (!string.IsNullOrEmpty(entity.Logo))
                    {
                        if (await _s3BucketService.FileExists(entity.Logo))
                        {
                            var response = await _s3BucketService.GetTags(entity.Logo);
                            var tags = response.Tagging;
                            if (!tags.Any(tag => tag.Key == "EntityType") && !tags.Any(tag => tag.Key == "EntityId"))
                            {
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityType", Value = EntityType.Series.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityId", Value = entity.Id.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "FileType", Value = FileTypeEnum.Logo.ToString() });
                            }

                            await _s3BucketService.UpdateTags(entity.Logo, tags);
                            _logger.LogInformation("Processed series = " + entity.Id.ToString());
                        }
                    }

                    if (!string.IsNullOrEmpty(entity.HomepageBanner))
                    {
                        if (await _s3BucketService.FileExists(entity.HomepageBanner))
                        {
                            var response = await _s3BucketService.GetTags(entity.HomepageBanner);
                            var tags = response.Tagging;
                            if (!tags.Any(tag => tag.Key == "EntityType") && !tags.Any(tag => tag.Key == "EntityId"))
                            {
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityType", Value = EntityType.Series.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "EntityId", Value = entity.Id.ToString() });
                                tags.Add(new Amazon.S3.Model.Tag { Key = "FileType", Value = FileTypeEnum.HomepageBanner.ToString() });
                            }

                            await _s3BucketService.UpdateTags(entity.HomepageBanner, tags);
                            _logger.LogInformation("Processed series = " + entity.Id.ToString());
                        }
                    }
                }

            } while (batch.Results.Count > 0);

            _logger.LogInformation("Completed series images. OK");
        }
    }



    public static class Extension
    {

        public static async Task<PagedResult<T>> GetBatch<T>(this IQueryable<T> query, int page, int pageSize) where T : class
        {
            var result = new PagedResult<T>
            {
                CurrentPage = page,
                PageSize = pageSize,
                RowCount = await query.CountAsync()
            };

            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (page - 1) * pageSize;
            result.Results = await query.Skip(skip).Take(pageSize).ToListAsync();

            return result;
        }
    }


    public abstract class PagedResultBase
    {
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
    }

    public class PagedResult<T> : PagedResultBase where T : class
    {
        public IList<T> Results { get; set; }

        public PagedResult()
        {
            Results = new List<T>();
        }
    }
}
