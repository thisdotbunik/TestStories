using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Aspose.Cells;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestStories.API.Common;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.Common.Configurations;
using TestStories.Common.Events;
using TestStories.Common.MailKit;
using TestStories.Common.Services.MailerLite;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;
using Newtonsoft.Json;

using Formatting = System.Xml.Formatting;

namespace TestStories.API.Services
{
    public class CommonWriteService : ICommonWriteService
    {
        readonly TestStoriesContext _context;
        readonly IPublishEvent<SendEmail> _event;
        readonly IMailerLiteService _mailerLiteService;
        readonly EmailSettings _emailSettings;
        readonly ILogger<CommonWriteService> _logger;
        readonly ImageSettings _imageSettings;
        readonly AppSettings _appSettings;
        readonly IS3BucketService _s3BucketService;
        readonly IServiceProvider _services;

        public CommonWriteService (IServiceProvider services, TestStoriesContext context, ILogger<CommonWriteService> logger,
            IPublishEvent<SendEmail> eEvent,
            IMailerLiteService mailerLiteService,
            IOptions<EmailSettings> emailSettings, IOptions<ImageSettings> imageSettings, IOptions<AppSettings> appSettings, IS3BucketService s3BucketService)
        {
            _services = services;
            _context = context;
            _logger = logger;
            _event = eEvent;
            _mailerLiteService = mailerLiteService;
            _emailSettings = emailSettings.Value;
            _imageSettings = imageSettings.Value;
            _appSettings = appSettings.Value;
            _s3BucketService = s3BucketService;
        }

        public async Task BecomeAPartnerMail (BecomeAPartnerMailModel mailModel)
        {
            if ( mailModel.Email != null )
            {
                var isEmail = Regex.IsMatch(mailModel.Email , @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z" , RegexOptions.IgnoreCase);
                if ( !isEmail )
                    throw new BusinessException("Please Enter Valid Email.");
            }

            var jsonString = LogsandException.GetCurrentInputJsonString(mailModel);
            _logger.LogDebug($"Send mail:request for commoncontroller and action:BecomeAPartnerMail input details:{jsonString}");

            // Send Mail Using SES
            var body = EmailTemplates.Templates[Templates.BecomeAPartnerAdmin]
           .Replace("{{company}}" , mailModel.Company)
           .Replace("{{name}}" , mailModel.Name)
           .Replace("{{title}}" , mailModel.Title)
           .Replace("{{message}}" , mailModel.Message)
           .Replace("{{email}}" , mailModel.Email)
           .Replace("{{phone}}" , mailModel.Phone);

            body = !string.IsNullOrEmpty(mailModel.PartnershipType) ? body.Replace("{{partnershipType}}" , mailModel.PartnershipType) : body.Replace("Interested in (Partnership Type):" , string.Empty);

            // Mail to Million Stories
            var emailAgency = new SendEmailBuilder(EnvironmentVariables.EmailQueue)
                .From(_emailSettings.From.noreply)
                .To(_emailSettings.From.partnership)
                .Subject(_emailSettings.Subject.BecomeAPartnerAdmin)
                .Action(EmailActions.BecomeAPartnerAdmin)
                .Body(body)
                .Build();
            await _event.Publish(emailAgency);

            // Mail to Partner

            // Send Mail Using SES
            var emailBody = EmailTemplates.Templates[Templates.BecomeAPartner]
           .Replace("{{firstName}}" , mailModel.Name.Split(" ").ToList()[0]);

            var email = new SendEmailBuilder(EnvironmentVariables.EmailQueue)
                .From(_emailSettings.From.noreply)
                .To(mailModel.Email)
                .Subject(_emailSettings.Subject.BecomeAPartner)
                .Action(EmailActions.BecomeAPartner)
                .Body(emailBody)
                .Build();

            await _event.Publish(email);
        }

        public async Task ContactUsMail (ContactUsMailModel mailModel)
        {
            if ( mailModel.Email != null )
            {
                var isEmail = Regex.IsMatch(mailModel.Email , @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z" , RegexOptions.IgnoreCase);
                if ( !isEmail )
                {
                    throw new BusinessException("Please Enter Valid Email.");
                }

            }
            var jsonString = LogsandException.GetCurrentInputJsonString(mailModel);
            _logger.LogDebug($"Received: request for commoncontroller and action:ContactUsMail input details:{jsonString}");

            // Mail To End User

            var body = EmailTemplates.Templates[Templates.ContactUs]
           .Replace("{{firstName}}" , mailModel.Name.Split(" ").ToList()[0])
           .Replace("{{message}}" , mailModel.Message);

            var email = new SendEmailBuilder(EnvironmentVariables.EmailQueue)
                .From(_emailSettings.From.noreply)
                .To(mailModel.Email)
                .Subject(_emailSettings.Subject.ContactUs)
                .Action(EmailActions.ContactUs)
                .Body(body)
                .Build();

            await _event.Publish(email);

            // Mail to Million Stories

            var emailBody = EmailTemplates.Templates[Templates.ContactUsAdmin]
           .Replace("{{name}}" , mailModel.Name)
           .Replace("{{email}}" , mailModel.Email)
           .Replace("{{phone}}" , mailModel.Phone)
           .Replace("{{message}}" , mailModel.Message);

            var emailAgency = new SendEmailBuilder(EnvironmentVariables.EmailQueue)
                .From(_emailSettings.From.noreply)
                .To(_emailSettings.From.contactus)
                .Subject(_emailSettings.Subject.ContactUsAdmin)
                .Action(EmailActions.ContactUsAdmin)
                .Body(emailBody)
                .Build();

            await _event.Publish(emailAgency);
        }

        private async Task<List<ExportMediaModel>> FilteredMedias(MediaFilter filter)
        {
            var items = new List<ExportMediaModel>();
            var mediaQuery =  (from media in _context.Media
                          .Include(x => x.MediaTopic)
                          .ThenInclude(x => x.Topic)
                          .Include(x => x.MediaTag)
                          .ThenInclude(x => x.Tag)
                          .Include(x=>x.ToolMedia)
                          .ThenInclude(x => x.Tool)
                          .Include(x => x.Mediastatus)
                          .Include(x => x.Mediatype)
                          .Include(x => x.PublishUser)
                          .Include(x => x.UploadUser)
                          .Include(x => x.Series)
                          .ThenInclude(x => x.ToolSeries)
                          .ThenInclude(x => x.Tool)
                          .Include(x => x.Source).Where(x => !x.IsDeleted).AsQueryable()                        
                          select new 
                          {
                              MediaId = media.Id,
                              MediaTitle = media.Name,
                              FeaturedImage = media.FeaturedImage,
                              ShortDesc = media.Description,
                              LongDesc = media.LongDescription,
                              Topic = media.MediaTopic.Select(x => x.Topic.Name),
                              Media_Tools = media.ToolMedia.Select(x => x.Tool.Name),
                              Series_Tools = media.Series.ToolSeries.Select(x => x.Tool.Name),
                              MediaType = media.Mediatype.Name,
                              MediaStatus = media.Mediastatus.Name,
                              PublishedBy = media.PublishUser.Name,
                              PublishDate = media.DatePublishedUtc,
                              SeriesId = media.SeriesId != 0 ? media.SeriesId.ToString() : "",
                              SeriesTitle = media.Series.Name,
                              Source = media.Source.Name,
                              DateCreated = media.DateCreatedUtc.ToString(),
                              UploadedBy = media.UploadUser.Name,
                              IsPrivate = media.IsPrivate ? "YES" : "NO",
                              ActiveDateFrom = media.ActiveFromUtc.HasValue ? media.ActiveFromUtc.ToString() : "",
                              ActiveDateTo = media.ActiveToUtc.HasValue ? media.ActiveToUtc.ToString() : "",
                              Tags = media.MediaTag.Select(x=>x.Tag.Name),
                              UploadedFileName = media.Metadata != null ? JsonConvert.DeserializeObject<MediaMetaData>(media.Metadata).name : "",
                              SeoFriendlyUrl = media.SeoUrl,
                              UniqueId = media.UniqueId
                          }).OrderByDescending(p => p.PublishDate).AsQueryable();

            if (!string.IsNullOrEmpty(filter.PublishFromDate) && !string.IsNullOrEmpty(filter.PublishToDate))
            {
                var publishFromDate = Convert.ToDateTime(Convert.ToDateTime(filter.PublishFromDate).ToString("MM/dd/yyyy")).Date;
                var publishToDate = Convert.ToDateTime(Convert.ToDateTime(filter.PublishToDate).ToString("MM/dd/yyyy")).Date;
                mediaQuery = mediaQuery.Where(item => item.PublishDate != null && Convert.ToDateTime(item.PublishDate).Date >= publishFromDate && Convert.ToDateTime(item.PublishDate).Date <= publishToDate);
            }

            if (!string.IsNullOrEmpty(filter.MediaStatus))
            {
                if (filter.MediaStatus.ToLower() != "all")
                    mediaQuery = mediaQuery.Where(item => item.MediaStatus == filter.MediaStatus);
            }
            if (!string.IsNullOrEmpty(filter.MediaType))
            {
                if (filter.MediaType.ToLower() != "all")
                    mediaQuery = mediaQuery.Where(item => item.MediaType == filter.MediaType);
            }
            if (!string.IsNullOrEmpty(filter.Source))
            {
                if (filter.Source.ToLower() != "all")
                    mediaQuery = mediaQuery.Where(item => item.Source == filter.Source);
            }

            if (!string.IsNullOrEmpty(filter.TopicName))
            {
                if (filter.TopicName.ToLower() != "all")
                    mediaQuery = mediaQuery.Where(item => item.Topic.Contains(filter.TopicName));
            }
            if (!string.IsNullOrEmpty(filter.SeriesName))
            {
                if (filter.SeriesName.ToLower() != "all")
                    mediaQuery = mediaQuery.Where(item => item.SeriesTitle == filter.SeriesName);
            }
            if (!string.IsNullOrEmpty(filter.UploadedBy))
            {
                if (filter.UploadedBy.ToLower() != "all")
                    mediaQuery = mediaQuery.Where(item => item.UploadedBy == filter.UploadedBy);
            }
            if (!string.IsNullOrEmpty(filter.PublishedBy))
            {
                if (filter.PublishedBy.ToLower() != "all")
                    mediaQuery = mediaQuery.Where(item => item.PublishedBy == filter.PublishedBy);
            }
            if (!string.IsNullOrEmpty(filter.SortedProperty) && !string.IsNullOrEmpty(filter.SortOrder))
            {
                _logger.LogDebug($"fulfilled request controller: CommonController and action: ExportMedias for sorted property: {filter.SortedProperty} and sorted property is : {filter.SortOrder}");


                if (filter.SortOrder.ToLower() == "descending" && filter.SortedProperty.ToLower() == "id")
                {
                    mediaQuery = mediaQuery.OrderByDescending(item => item.MediaId);
                }
                if (filter.SortOrder.ToLower() == "ascending" && filter.SortedProperty.ToLower() == "id")
                {
                    mediaQuery = mediaQuery.OrderBy(item => item.MediaId);
                }

                if (filter.SortOrder.ToLower() == "descending" && filter.SortedProperty.ToLower() == "title")
                {
                    mediaQuery = mediaQuery.OrderByDescending(item => item.MediaTitle);
                }
                if (filter.SortOrder.ToLower() == "ascending" && filter.SortedProperty.ToLower() == "title")
                {
                    mediaQuery = mediaQuery.OrderBy(item => item.MediaTitle);
                }
                if (filter.SortOrder.ToLower() == "descending" && filter.SortedProperty.ToLower() == "status")
                {
                    mediaQuery = mediaQuery.OrderByDescending(item => item.MediaStatus);
                }
                if (filter.SortOrder.ToLower() == "ascending" && filter.SortedProperty.ToLower() == "status")
                {
                    mediaQuery = mediaQuery.OrderBy(item => item.MediaStatus);
                }
                if (filter.SortOrder.ToLower() == "descending" && filter.SortedProperty.ToLower() == "mediatype")
                {
                    mediaQuery = mediaQuery.OrderByDescending(item => item.MediaType);
                }
                if (filter.SortOrder.ToLower() == "ascending" && filter.SortedProperty.ToLower() == "mediatype")
                {
                    mediaQuery = mediaQuery.OrderBy(item => item.MediaType);
                }
                if (filter.SortOrder.ToLower() == "descending" && filter.SortedProperty.ToLower() == "date")
                {
                    mediaQuery = mediaQuery.OrderByDescending(item => item.PublishDate);
                }
                if (filter.SortOrder.ToLower() == "ascending" && filter.SortedProperty.ToLower() == "date")
                {
                    mediaQuery = mediaQuery.OrderBy(item => item.PublishDate);
                }

                if (filter.SortOrder.ToLower() == "descending" && filter.SortedProperty.ToLower() == "source")
                {
                    mediaQuery = mediaQuery.OrderByDescending(item => item.Source);
                }
                if (filter.SortOrder.ToLower() == "ascending" && filter.SortedProperty.ToLower() == "source")
                {
                    mediaQuery = mediaQuery.OrderBy(item => item.Source);
                }
                if (filter.SortOrder.ToLower() == "descending" && filter.SortedProperty.ToLower() == "uploadedby")
                {
                    mediaQuery = mediaQuery.OrderByDescending(item => item.UploadedBy);
                }
                if (filter.SortOrder.ToLower() == "ascending" && filter.SortedProperty.ToLower() == "uploadedby")
                {
                    mediaQuery = mediaQuery.OrderBy(item => item.UploadedBy);
                }

                if (filter.SortOrder.ToLower() == "descending" && filter.SortedProperty.ToLower() == "publisedby")
                {
                    mediaQuery = mediaQuery.OrderByDescending(item => item.PublishedBy);
                }
                if (filter.SortOrder.ToLower() == "ascending" && filter.SortedProperty.ToLower() == "publisedby")
                {
                    mediaQuery = mediaQuery.OrderBy(item => item.PublishedBy);
                }              
            }

            items = mediaQuery.Select(media => new ExportMediaModel
            {
                MediaId = media.MediaId,
                MediaTitle = media.MediaTitle,
                FeaturedImage = media.FeaturedImage,
                ShortDesc = media.ShortDesc,
                LongDesc = media.LongDesc,
                Topic = media.Topic.ToList().Count > 0 ? string.Join(",", media.Topic.ToList()) : "",
                LinkedResources = media.Media_Tools.AsEnumerable().Union(media.Series_Tools).ToList().Count > 0 ? string.Join("", media.Media_Tools.AsEnumerable().Union(media.Series_Tools).ToList()) : "",
                MediaType = media.MediaType,
                MediaStatus = media.MediaStatus,
                PublishedBy = media.PublishedBy,
                PublishDate = media.PublishDate,
                SeriesId = media.SeriesId,
                SeriesTitle = media.SeriesTitle,
                Source = media.Source,
                DateCreated = media.DateCreated,
                UploadedBy = media.UploadedBy,
                IsPrivate = media.IsPrivate,
                ActiveDateFrom = media.ActiveDateFrom,
                ActiveDateTo = media.ActiveDateTo,
                Tags = media.Tags.ToList().Count > 0 ? string.Join(",", media.Tags.ToList()) : "",
                UploadedFileName = media.UploadedFileName,
                SeoFriendlyUrl = media.SeoFriendlyUrl,
                UniqueId = media.UniqueId
            }).ToList();

            foreach (var media in items)
            {
                var featuredImages = await _s3BucketService.GetCompressedImages(media.FeaturedImage, EntityType.Media);
                media.FeaturedImage = featuredImages.Banner;
            }

            return items;
        }

        public async void FixVideoDuration()
        {
            try
            {
                _logger.LogInformation("Started FixVideoDuration");
                using (var scope = _services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<TestStoriesContext>();
                    var culture = new CultureInfo("en-US");
                    var medias = await context.Media.Where(x => x.MediatypeId == (int)MediaTypeEnum.Video || x.MediatypeId == (int)MediaTypeEnum.PodcastAudio).OrderBy(x => x.Id).ToListAsync();
                    var ffProbe = new NReco.VideoInfo.FFProbe { FFProbeExeName = "ffprobe", ToolPath = "/usr/bin/" };

                    foreach (var x in medias)
                    {
                        if (x.Metadata != null)
                        {
                            var metadata = JsonConvert.DeserializeObject<MediaMetaData>(x.Metadata);
                            try
                            {
                                var mediaInfo = ffProbe.GetMediaInfo($"https://{ EnvironmentVariables.CDN_DNS_VIDEO_TRANSCODED }/{x.HlsUrl}");
                                metadata.duration = mediaInfo.Duration.ToString(@"hh\:mm\:ss", culture);
                                x.Metadata = JsonConvert.SerializeObject(metadata);
                                context.Media.Update(x);
                                _logger.LogInformation($"OK. Updated media {x.Id}, " + $"https://{ EnvironmentVariables.CDN_DNS_VIDEO_TRANSCODED }/{x.HlsUrl}");
                                await context.SaveChangesAsync();
                            }
                            catch(Exception)
                            {
                                _logger.LogError($"Url not valid. Media {x.Id}, " + $"https://{ EnvironmentVariables.CDN_DNS_VIDEO_TRANSCODED }/{x.HlsUrl}");
                            }
                        }
                    }
                }
                _logger.LogInformation("Completed FixVideoDuration. OK");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task FixVideoSize ()
        {
            _logger.LogInformation("Started FixVideoSize");

             var medias = await _context.Media.Where(x => x.MediatypeId == (int)MediaTypeEnum.Video ).OrderBy(x => x.Id).ToListAsync();

            try
            {
                var lstMedias = new List<Media>();
                foreach ( var x in medias )
                {
                    if ( x.Metadata != null )
                    {
                        var metadata = JsonConvert.DeserializeObject<MediaMetaData>(x.Metadata);

                        var result = await _s3BucketService.GetFileDetail(x.Url);
                        if ( result != null )
                        {
                            var mediaSize = Math.Round(Convert.ToDecimal(( result.Size/1024 )/1024.00) , 2);
                            metadata.size = $"{mediaSize}Mb";
                            x.Metadata = JsonConvert.SerializeObject(metadata);
                            lstMedias.Add(x);
                        }
                    }
                }
                if ( lstMedias.Count > 0 )
                {
                    _context.Media.UpdateRange(lstMedias);
                    await _context.SaveChangesAsync();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex , ex.Message);
            }
        }

        public async Task FixAudioSize ()
        {
            _logger.LogInformation("Started FixAudioSize");

            var medias = await _context.Media.Where(x => x.MediatypeId == (int)MediaTypeEnum.PodcastAudio).OrderBy(x => x.Id).ToListAsync();

            try
            {
                var lstMedias = new List<Media>();
                foreach ( var x in medias )
                {
                    if ( x.Metadata != null )
                    {
                        var metadata = JsonConvert.DeserializeObject<AudioMetaData>(x.Metadata);

                        var updatedMetaData = new MediaMetaData
                        {
                            name = metadata.name ,
                            type = metadata.mediaType ,
                            size = metadata.size ,
                            duration = metadata.duration ,
                            lastModifiedDate = metadata.lastModifiedDate
                        };

                        var result = await _s3BucketService.GetFileDetail(x.Url);
                        if ( result != null )
                        {
                            var mediaSize = Math.Round(Convert.ToDecimal(( result.Size/1024 )/1024.00) , 2);
                            updatedMetaData.size = $"{mediaSize}Mb";
                            x.Metadata = JsonConvert.SerializeObject(updatedMetaData);
                            lstMedias.Add(x);
                        }
                    }
                }
                if ( lstMedias.Count > 0 )
                {
                    _context.Media.UpdateRange(lstMedias);
                    await _context.SaveChangesAsync();
                }
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex , ex.Message);
            }
        }

        public async Task GenerateMediaSiteMap ()
        {
            var medias = await (_context.Media.Where(x => x.IsVisibleOnGoogle && x.MediatypeId == (int)MediaTypeEnum.Video &&
                                                          x.MediastatusId == (int)MediaStatusEnum.Published && !x.IsDeleted &&
                                                         (x.ActiveFromUtc.HasValue && x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= x.ActiveToUtc :
                                                          x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue ? x.ActiveFromUtc <= DateTime.UtcNow :
                                                         x.ActiveFromUtc.HasValue || !x.ActiveToUtc.HasValue || DateTime.UtcNow <= x.ActiveToUtc))
                  .Select(x => new MediaSiteMap
                  {
                      Id = x.Id,
                      Title = x.Name,
                      Description = x.Description,
                      Thumbnail = x.Thumbnail,
                      PublishDate = x.DatePublishedUtc,
                      Url = x.Url,
                      HlsUrl = x.HlsUrl,
                      SeoUrl = x.SeoUrl,
                      ExpireDate = x.ActiveToUtc,
                      MetaData = x.Metadata != null ? JsonConvert.DeserializeObject<MediaMetaData>(x.Metadata) : null
                  })).OrderBy(x => x.Id).ToListAsync();


            var builder = new StringBuilder();
            using (var stringWriter = new StringWriter(builder))
            {
                using var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset");
                writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                writer.WriteAttributeString("xmlns", "video", null, "http://www.google.com/schemas/sitemap-video/1.0");

                foreach (var x in medias)
                {
                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", $"https://{_appSettings.ClientUiDomain}/media/{x.SeoUrl}/?video={x.Id}");
                    writer.WriteStartElement("video", "video", null);
                    writer.WriteElementString("video", "title", null, x.Title);
                    writer.WriteElementString("video", "description", null, x.Description);
                    writer.WriteElementString("video", "thumbnail_loc", null, !string.IsNullOrEmpty(x.Thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(x.Thumbnail, _imageSettings.Media.Thumbnail) : string.Empty);
                    writer.WriteElementString("video", "content_loc", null, $"https://{ EnvironmentVariables.CDN_DNS_VIDEO_TRANSCODED }/{x.Url}");
                    if (x.PublishDate.HasValue)
                    {
                        writer.WriteElementString("video", "publication_date", null, ConvertDateToW3CTime(x.PublishDate.Value));
                    }

                    if (x.ExpireDate.HasValue)
                    {
                        writer.WriteElementString("video", "expiration_date", null, ConvertDateToW3CTime(x.ExpireDate.Value));
                    }

                    if (x.MetaData != null && !string.IsNullOrEmpty(x.MetaData.duration))
                    {
                        try
                        {
                            var duration = TimeSpan.Parse(x.MetaData.duration);
                            writer.WriteElementString("video", "duration", null, duration.TotalSeconds.ToString());
                        }
                        catch { }
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }

            await _s3BucketService.UploadXmlFileAsync(builder.Replace("\"utf-16\"", "\"utf-8\"").ToString(), "sitemap/sitemap.xml");
        }

        public async Task GenerateSeoFriendlyUrl (bool isAllUpdate)
        {
            var medias = new List<Media>();
            var topics = new List<Topic>();
            var series = new List<Series>();
            if ( isAllUpdate )
            {
                medias = _context.Media.ToList();
                topics = _context.Topic.ToList();
                series = _context.Series.ToList();
            }
            else
            {
                medias = _context.Media.Where(x => x.SeoUrl == null || x.SeoUrl == "").ToList();
                topics = _context.Topic.Where(x => x.SeoUrl == null || x.SeoUrl == "").ToList();
                series = _context.Series.Where(x => x.SeoUrl == null || x.SeoUrl == "").ToList();
            }

            Parallel.ForEach(medias , async media =>
            {
                media.SeoUrl = Helper.SeoFriendlyUrl(media.Name);
            });

            _context.Media.UpdateRange(medias);


            Parallel.ForEach(topics , async topic =>
            {
                topic.SeoUrl = Helper.SeoFriendlyUrl(topic.Name);
            });
            _context.Topic.UpdateRange(topics);


            Parallel.ForEach(series , async serie =>
            {
                serie.SeoUrl = Helper.SeoFriendlyUrl(serie.Name);
            });
            _context.Series.UpdateRange(series);
            _context.SaveChanges();
        }

        public async Task MigrateSrtFiles (List<AddSrtFileModel> srtFiles)
        {
            var newSrtFiles = new List<MediaSrt>();

            foreach ( var item in srtFiles )
            {
                var isSrtExist = _context.MediaSrt.Any(x => x.MediaId == item.MediaId && x.File == item.File);
                if ( !isSrtExist )
                {
                    newSrtFiles.Add(new MediaSrt { File = item.File , FileMetadata = item.FileMetadata , Language = item.Language , MediaId = item.MediaId });
                }
            }

            if ( newSrtFiles.Count > 0 )
            {
                _logger.LogDebug($"Request received Controller:EfMediaRepository and Action:migrateSrtFiles, MediaIds: {  string.Join(", " , newSrtFiles.Select(x => x.MediaId))}");
                _context.MediaSrt.AddRange(newSrtFiles);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<NewletterResponse> SubscribeNewsletter (int userId , SubscribeNewletterModel model)
        {
            var response = new NewletterResponse();

            if ( userId != 0 )
            {
                var registerUser = await _context.User.SingleOrDefaultAsync(x => x.Id == userId);
                if ( registerUser != null )
                {
                    if ( registerUser.IsNewsletterSubscribed )
                    {
                        throw new BusinessException("Newsletter already subscribed");
                    }
                    var user = await SubscribeNewsletter(registerUser.Id);
                    if ( user != null )
                    {
                        response.UserId = user.Id;
                        response.FirstName = user.FirstName;
                        response.LastName = user.LastName;
                        response.UserName = user.Email;
                        response.IsNewsletterSubsribed = user.IsNewsletterSubscribed;
                    }
                }
            }

            var computedName = Regex.Replace(model.Name.Trim() , @"\s+" , " ");
            var firstName = computedName.Split(" ").FirstOrDefault();
            var lastName = computedName.IndexOf(" ") > 0 ? computedName.Substring(computedName.IndexOf(" ")) : "";

            var tag = "Homepage Form";
            if ( model.Source == SubscriptionSource.Subscriptionpage.ToString() )
            {
                tag = "Email Subscription Form";
            }

            var isSuccess = await _mailerLiteService.Subscribe(new SubscribeRequest() { Email = model.Email , FirstName = firstName , LastName = lastName , Tag = tag });
            if(!isSuccess)
            {
                throw new BusinessException("Error while subscribing Newsletter");
            }    
            response.IsNewsletterSubsribed = true;
            return response;
        }

        #region Private Methods
        private async Task<User> SubscribeNewsletter (int userId)
        {
            var userDetail = await _context.User.SingleOrDefaultAsync(x => x.Id == userId);
            if ( userDetail != null )
            {
                userDetail.IsNewsletterSubscribed = true;
                await _context.SaveChangesAsync();
                return userDetail;
            }
            return null;
        }

        private static string ConvertDateToW3CTime(DateTime date)
        {
            var utcOffset = TimeZoneInfo.Local.GetUtcOffset(date);
            var w3CTime = date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
            w3CTime += utcOffset == TimeSpan.Zero ? "Z" : string.Format("{0}{1:00}:{2:00}", (utcOffset > TimeSpan.Zero ? "+" : "-") , utcOffset.Hours, utcOffset.Minutes);
            return w3CTime;
        }
        private async Task ExportJsonToExcel (List<ExportMediaModel> medias)
        {
            var book = new Workbook();
            var sheet = book.Worksheets[0];
            sheet.Cells.ImportCustomObjects((System.Collections.ICollection)medias ,
                new string[] { "MediaId" , "MediaTitle" , "FeaturedImage" , "ShortDesc" , "LongDesc" , "Topic" , "LinkedResources" , "MediaType" , "MediaStatus" , "PublishedBy" , "SeriesId" , "SeriesTitle" , "Source" , "DateCreated" , "UploadedBy" , "IsPrivate" , "ActiveDateFrom" , "ActiveDateTo" , "Tags" , "UploadedFileName" , "SeoFriendlyUrl" , "UniqueId" } ,
                true , // isPropertyNameShown
                0 , // firstRow
                0 , // firstColumn
                medias.Count , // Number of objects to be exported
                true , // insertRows
                null , // dateFormatString
                false); // convertStringToNumber
                        // Save the Excel file
            var stream = book.SaveToStream();
            await _s3BucketService.UploadExcelFileAsync(stream , $"{EnvironmentVariables.Env}-media");
        }

        private static async Task<byte[]> ExportUserDataJsonToExcel (List<ExportUserDataModel> usersData)
        {
            byte[] content;
            var book = new Workbook();
            var sheet = book.Worksheets[0];
            sheet.Cells.ImportCustomObjects((System.Collections.ICollection)usersData ,
                new string[] { "UserId" , "FullName" , "Email" , "NewsletterSubscribed" , "SignupDateUtc" , "Playlists" , "SubscribedSeries" , "SubscribedTopics" , "Favorites" } ,
                true , // isPropertyNameShown
                0 , // firstRow
                0 , // firstColumn
                usersData.Count , // Number of objects to be exported
                true , // insertRows
                null , // dateFormatString
                false); // convertStringToNumber
                        // Save the Excel file
            using ( var stream = new MemoryStream() )
            {
                book.Save(stream , SaveFormat.Xlsx);
                content = stream.ToArray();
            }
            return content;
        }

        private async Task<List<ExportUserDataModel>> GetUserSpecificData()
        {
            var items = ( from user in _context.User
                          where user.UserstatusId == (int)UserStatusEnum.Active

                          let playlists = (from playlist in _context.Playlist.Where(x => x.UserId == user.Id)
                                            select playlist.Name).ToList()

                          let subscribedSeries = (from subs in _context.SubscriptionSeries
                                                   join series in _context.Series
                                                   on subs.SeriesId equals series.Id where user.Id == subs.UserId
                                                   select series.Name).ToList()

                          let subscribedTopics = (from subs in _context.SubscriptionTopic
                                                   join topic in _context.Topic
                                                   on subs.TopicId equals topic.Id
                                                   where user.Id == subs.UserId
                                                   select topic.Name).ToList()

                          let favorites = (from fav in _context.Favorites
                                                   join media in _context.Media
                                                   on fav.MediaId equals media.Id
                                                   where user.Id == fav.UserId && media.MediastatusId != (int)MediaStatusEnum.Archived
                                                   && !media.IsDeleted
                                                   && ( media.ActiveFromUtc.HasValue &&  media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc :
                                                   media.ActiveFromUtc.HasValue && !media.ActiveToUtc.HasValue ? media.ActiveFromUtc <= DateTime.UtcNow :
                                                   media.ActiveFromUtc.HasValue || !media.ActiveToUtc.HasValue || DateTime.UtcNow <= media.ActiveToUtc )
                                           select media.Name).ToList()
                          select new ExportUserDataModel
                          {
                              UserId = user.Id ,
                              FullName = user.Name ,
                              Email = user.Email ,
                              NewsletterSubscribed = user.IsNewsletterSubscribed ? "YES" : "NO" ,
                              SignupDateUtc = user.DateCreatedUtc.Date.ToString(),
                              Playlists = playlists.Count > 0 ? string.Join("," , playlists) : "" ,
                              SubscribedSeries = subscribedSeries.Count > 0 ? string.Join("," , subscribedSeries) : "" ,
                              SubscribedTopics = subscribedTopics.Count > 0 ? string.Join("," , subscribedTopics) : "" ,
                              Favorites = favorites.Count > 0 ? string.Join("," , favorites) : "" ,
                          } ).OrderBy(p => p.UserId).ToList();
            return items;
        }
        public async Task ExportMedias (MediaFilter filter)
        {
            var result = await FilteredMedias(filter);
            await ExportJsonToExcel(result);
        }

        public async Task<byte[]> ExportUsersSubscribedData()
        {
            var users = await GetUserSpecificData();
            return await ExportUserDataJsonToExcel(users);
        }

        #endregion
    }
}
