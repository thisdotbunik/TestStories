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
    public class PartnerReadService : IPartnerReadService
    {
        private readonly TestStoriesContext _context;
        private readonly IS3BucketService _s3BucketService;

        /// <inheritdoc />
        public PartnerReadService (TestStoriesContext ctx, IS3BucketService s3BucketService)
        {
            _context = ctx;
            _s3BucketService = s3BucketService;
        }
        public async Task<CollectionModel<DistributionAutocompleteViewModel>> DistributionMediaAutoCompleteSearch ()
        {
            var mediaNames = ( from x in _context.PartnerMedia.ToList()
                               let mediaName = _context.Media.Where(t => t.Id == x.MediaId).FirstOrDefault().Name
                               select new DistributionAutocompleteViewModel
                               {
                                   Name = mediaName ,
                               } ).ToList();

            var mediaIds = ( from x in _context.PartnerMedia.ToList()
                             select new DistributionAutocompleteViewModel
                             {
                                 Name = x.MediaId.ToString() ,
                             } ).ToList();
            var mediaItems = mediaNames.Union(mediaIds , new PartnerMediaComparer()).ToList();

            var partnerItems = ( from x in _context.PartnerMedia.ToList()
                                 let partner = _context.Partner.Where(t => t.Id == x.PartnerId).ToList()
                                 select new DistributionAutocompleteViewModel
                                 {
                                     Name = partner.FirstOrDefault().Name
                                 } ).ToList();

            var totalItems = mediaItems.Union(partnerItems).GroupBy(x => x.Name).OrderBy(y => y.Key).ToList();

            var allItem = ( from temp in totalItems
                            select new DistributionAutocompleteViewModel
                            {
                                Name = temp.Key
                            } ).ToList();

            var allItems = new CollectionModel<DistributionAutocompleteViewModel>
            {
                Items = allItem ,
                TotalCount = allItem.Count
            };

            return allItems;
        }

        public async Task<PartnerDistributionDetailsViewModel> DistributionMediaSearch (FilterPartnerDistributionViewRequest _filter)
        {
            var isIdNumeric = IsNumeric(_filter.FilterString);
            IQueryable<PartnerMedia> partnerMedia;
            List<PartnerDistributionViewModel> result;
            if ( isIdNumeric )
            {
                var id = Convert.ToInt32(_filter.FilterString);
                partnerMedia = _context.PartnerMedia.Where(x => x.MediaId == id && x.IsExpired == false);
            }
            else
            {
                partnerMedia = _context.PartnerMedia.Where(x => x.IsExpired == false);
            }
            var skip = _filter.PageSize * ( _filter.Page - 1 );
            var items = await ( from x in partnerMedia
                                let mediaName = _context.Media.Where(t => t.Id == x.MediaId).FirstOrDefault().Name
                                let partner = _context.Partner.Where(t => t.Id == x.PartnerId)
                                select new PartnerDistributionViewModel
                                {
                                    Id = x.Id ,
                                    MediaId = x.MediaId ,
                                    Title = mediaName ,
                                    Partner = partner.FirstOrDefault().Name ,
                                    ShareWith = x.Email ,
                                    StartDate = x.StartDateUtc ,
                                    EndDate = x.EndDateUtc ,
                                    PartnerId = x.PartnerId
                                } ).OrderByDescending(x => x.StartDate).ToListAsync();

            if ( !isIdNumeric )
            {
                if ( !string.IsNullOrEmpty(_filter.FilterString) )
                {
                    items = items.Where(x => x.Partner.ToLower().Contains(_filter.FilterString.ToLower()) || x.Title.ToLower().Contains(_filter.FilterString.ToLower())).ToList();
                }
            }

            if ( !string.IsNullOrEmpty(Convert.ToString(_filter.SortedProperty)) && !string.IsNullOrEmpty(Convert.ToString(_filter.SortOrder)) )
            {
                // filter by title
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "title" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.Title).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "title" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.Title).ToList();
                }

                //filter by partner
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "partner" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.Partner).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "partner" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.Partner).ToList();
                }

                //filter by ShareWith
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "sharewith" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.ShareWith).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "sharewith" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.ShareWith).ToList();
                }

                //filter by StartDate
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "startdate" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.StartDate).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "startdate" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.StartDate).ToList();
                }
                //filter by EndDate
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "enddate" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.EndDate).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "enddate" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.EndDate).ToList();
                }
            }


            if ( _filter.Page != 0 && _filter.PageSize != 0 )
            {
                result = items.Skip(skip).Take(_filter.PageSize).ToList();
            }
            else
            {
                result = items;
            }

            return new PartnerDistributionDetailsViewModel
            {
                Items = result ,
                Count = result.Count
            };
        }

        public async Task<CollectionModel<PartnerModel>> GetActivePartnersAsync ()
        {
            var result = await ( from x in _context.Partner
                                 where !x.IsArchived
                                 select new PartnerModel
                                 {
                                     Id = x.Id ,
                                     Name = x.Name ,
                                     Description = x.Description ,
                                     Logo = x.Logo ,
                                     ShowOnPartner = x.ShowOnPartnerPage ,
                                     Link = x.Link ,
                                     OrderNumber = x.OrderNumber
                                 } ).OrderBy(x => x.Name).ToListAsync();

            foreach ( var partner in result )
            {
                partner.Logo = !string.IsNullOrEmpty(partner.Logo) ? _s3BucketService.RetrieveImageCDNUrl(partner.Logo) : string.Empty;
            }

            return new CollectionModel<PartnerModel> { Items = result , TotalCount = result.Count };
        }

        public async Task<PartnerMedia> GetDistributionMedia (long mediaId , int partnerId)
        {
            return  await _context.PartnerMedia.SingleOrDefaultAsync(x => x.MediaId == mediaId && x.PartnerId == partnerId && x.IsExpired == false);
        }

        public async Task<PartnerDetailViewModel> GetPartnerAsync (int id)
        {
            var query = _context.Partner.Where(x => x.Id == id);
            var partnerTypeIds = ( from t1 in _context.PartnerPartnerType.Where(p => p.PartnerId == id)
                                   join t2 in _context.PartnerType
                                   on t1.PartnertypeId equals t2.Id
                                   select t2.Id ).ToList();
            var result = await query.SingleOrDefaultAsync();
            if ( result != null )
            {
                var response = new PartnerDetailViewModel
                {
                    Id = result.Id ,
                    Name = result.Name ,
                    PartnerTypeIds = partnerTypeIds ,
                    Description = result.Description ,
                    DateAdded = result.DateAddedUtc ,
                    Logo = !string.IsNullOrEmpty(result.Logo) ? _s3BucketService.RetrieveImageCDNUrl(result.Logo) : string.Empty ,
                    LogoFileName = result.LogoMetadata ,
                    ShowOnPartnerPage = result.ShowOnPartnerPage ,
                    IsArchived = result.IsArchived ,
                    Link = result.Link
                };

                return response;
            }
            return null;
        }

        public async Task<PartnerDistributionDetailsViewModel> GetPartnerDistribution (FilterPartnerDistributionViewRequest _filter)
        {
            var skip = _filter.PageSize * ( _filter.Page - 1 );
            List<PartnerMedia> partnerMedia;
            var result = new List<PartnerDistributionViewModel>();
            if ( _filter.id != 0 )
            {
                partnerMedia = _context.PartnerMedia.Where(x => x.PartnerId == _filter.id && x.IsExpired == false).ToList();
            }
            else
            {
                partnerMedia = _context.PartnerMedia.Where(x => x.IsExpired == false).ToList();
            }

            var items =  ( from x in partnerMedia.ToList()
                                let mediaName = _context.Media.Where(t => t.Id == x.MediaId).FirstOrDefault().Name
                                let partner = _context.Partner.Where(t => t.Id == x.PartnerId).ToList()
                                select new PartnerDistributionViewModel
                                {
                                    Id = x.Id ,
                                    MediaId = x.MediaId ,
                                    Title = mediaName ,
                                    Partner = partner.FirstOrDefault().Name ,
                                    ShareWith = x.Email ,
                                    StartDate = x.StartDateUtc ,
                                    EndDate = x.EndDateUtc ,
                                    PartnerId = x.PartnerId
                                }).ToList();


            if ( !string.IsNullOrEmpty(_filter.FilterString) )
            {
                items = items.Where(x => x.Partner.ToLower().Contains(_filter.FilterString.ToLower())
                || x.Title.ToLower().Contains(_filter.FilterString.ToLower()) || x.MediaId.ToString() == _filter.FilterString).ToList();
            }


            if ( !string.IsNullOrEmpty(Convert.ToString(_filter.SortedProperty)) && !string.IsNullOrEmpty(Convert.ToString(_filter.SortOrder)) )
            {
                // filter by title
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "title" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.Title).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "title" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.Title).ToList();
                }

                //filter by mediaId
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "mediaid" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.MediaId).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "mediaid" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.MediaId).ToList();
                }

                //filter by partner
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "partner" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.Partner).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "partner" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.Partner).ToList();
                }

                //filter by ShareWith
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "sharewith" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.ShareWith).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "sharewith" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.ShareWith).ToList();
                }

                //filter by StartDate
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "startdate" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.StartDate).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "startdate" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.StartDate).ToList();
                }
                //filter by EndDate
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "enddate" && Convert.ToString(_filter.SortOrder).ToLower() == "descending" )
                {
                    items = items.OrderByDescending(x => x.EndDate).ToList();
                }
                if ( Convert.ToString(_filter.SortedProperty).ToLower() == "enddate" && Convert.ToString(_filter.SortOrder).ToLower() == "ascending" )
                {
                    items = items.OrderBy(x => x.EndDate).ToList();
                }
            }

            if ( _filter.PageSize != 0 && _filter.Page != 0 )
            {
                result = items;
                result = result.Skip(skip).Take(_filter.PageSize).ToList();
            }

            return new PartnerDistributionDetailsViewModel
            {
                Items = result ,
                Count = items.Count
            };
        }

        public async Task<CollectionModel<PartnerDetailViewModel>> GetPartnerDetails (int? id , int page , int pageSize)
        {
            var result = new List<PartnerDetailViewModel>();
           
            var partnerItems =  await _context.Partner.ToListAsync();
            if ( id != null )
            {
                partnerItems =   partnerItems.Where(x => x.Id == id).ToList();
            }

            var items =  ( from x in partnerItems
                                let partnerType = from t1 in _context.PartnerPartnerType.Where(p => p.PartnerId == x.Id)
                                                  join t2 in _context.PartnerType
                                                  on t1.PartnertypeId equals t2.Id
                                                  select t2.Name
                                select new PartnerDetailViewModel
                                {
                                    Id = x.Id ,
                                    Name = x.Name ,
                                    PartnerType = partnerType.ToList() ,
                                    DateAdded = x.DateAddedUtc ,
                                    IsArchived = x.IsArchived ,
                                    ShowOnPartnerPage = x.ShowOnPartnerPage ,
                                    Description = x.Description ,
                                    Logo = x.Logo
                                } ).ToList();

            if ( page != 0 && pageSize != 0 )
            {
                var skip = pageSize * ( page - 1 );
                result = items.Skip(skip).Take(pageSize).OrderBy(x => x.Name).ToList();
            }

            foreach ( var partner in result )
            {
                partner.Logo = !string.IsNullOrEmpty(partner.Logo) ? _s3BucketService.RetrieveImageCDNUrl(partner.Logo) : string.Empty;
            }

            return new CollectionModel<PartnerDetailViewModel> { Items = result , TotalCount = items.Count };
        }

        public async Task<PartnerViewModel> GetPartnerMedia (int id , int PageSize , int Page)
        {
            var result = await ( from x in _context.Media.Where(x => x.SourceId == id && !x.IsDeleted)
                           let mediaStatus = _context.MediaStatus.Where(t => t.Id == x.MediastatusId).FirstOrDefault().Name
                           let mediaType = _context.MediaType.Where(t => t.Id == x.MediatypeId).FirstOrDefault().Name
                           let sourceName = _context.Partner.Where(t => t.Id == x.SourceId).FirstOrDefault().Name
                           let editor = _context.User.Where(t => t.Id == x.UploadUserId).FirstOrDefault().Name
                           let publishBy = _context.User.Where(t => t.Id == x.PublishUserId).FirstOrDefault().Name
                           select new MediaPartnerViewModel
                           {
                               Id = x.Id,
                               Title = x.Name,
                               Status = mediaStatus,
                               MediaType = mediaType,
                               PublishDate = Convert.ToDateTime(x.DatePublishedUtc),
                               Source = sourceName,
                               Editor = editor,
                               PublishBy = publishBy,
                               IsVisibleOnGoogle = x.IsVisibleOnGoogle
                           } ).ToListAsync();

            if ( Page != 0 && PageSize != 0 )
            {
                var skip = PageSize * ( Page - 1 );
                result = result.Skip(skip).Take(PageSize).ToList();
            }
            var PartnerDetails = _context.Partner.Where(t => t.Id == id).FirstOrDefault();
            var uidLogo = !string.IsNullOrEmpty(PartnerDetails.Logo) ? _s3BucketService.RetrieveImageCDNUrl(PartnerDetails.Logo) : string.Empty;

            return new PartnerViewModel
            {
                Logo = uidLogo ,
                Items = result ,
                PartnerName = PartnerDetails == null ? "" : PartnerDetails.Name ,
                Count = result.Count
            };
        }

        public async Task<CollectionModel<PartnerMediaViewModel>> GetPartnerMedia (PartnerMediaFilterRequest request)
        {
            var totalCount = 0;
            var lstPartnerMedia = await PartnerMedia(request.PartnerId);
            if ( !string.IsNullOrEmpty(request.FilterString) )
            {
                lstPartnerMedia = lstPartnerMedia.Where(x => x.Id.ToString().Contains(request.FilterString.ToLower()) || x.Name.ToLower().Contains(request.FilterString.ToLower())).ToList();
            }
            totalCount = lstPartnerMedia.Count;
            if ( !string.IsNullOrEmpty(Convert.ToString(request.SortedProperty)) && !string.IsNullOrEmpty(Convert.ToString(request.SortOrder)) )
            {
                // filter by mediId
                if ( Convert.ToString(request.SortedProperty).ToLower() == "id" && Convert.ToString(request.SortOrder).ToLower() == "descending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderByDescending(x => x.Id).ToList();
                }
                if ( Convert.ToString(request.SortedProperty).ToLower() == "id" && Convert.ToString(request.SortOrder).ToLower() == "ascending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderBy(x => x.Id).ToList();
                }

                // filter by title
                if ( Convert.ToString(request.SortedProperty).ToLower() == "name" && Convert.ToString(request.SortOrder).ToLower() == "descending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderByDescending(x => x.Name).ToList();
                }
                if ( Convert.ToString(request.SortedProperty).ToLower() == "name" && Convert.ToString(request.SortOrder).ToLower() == "ascending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderBy(x => x.Name).ToList();
                }

                //filter by partner
                if ( Convert.ToString(request.SortedProperty).ToLower() == "mediastatus" && Convert.ToString(request.SortOrder).ToLower() == "descending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderByDescending(x => x.MediaStatus).ToList();
                }
                if ( Convert.ToString(request.SortedProperty).ToLower() == "mediastatus" && Convert.ToString(request.SortOrder).ToLower() == "ascending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderBy(x => x.MediaStatus).ToList();
                }

                //filter by Created Date
                if ( Convert.ToString(request.SortedProperty).ToLower() == "createddate" && Convert.ToString(request.SortOrder).ToLower() == "descending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderByDescending(x => x.CreatedDate).ToList();
                }
                if ( Convert.ToString(request.SortedProperty).ToLower() == "createddate" && Convert.ToString(request.SortOrder).ToLower() == "ascending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderBy(x => x.CreatedDate).ToList();
                }
                //filter by Publish Date
                if ( Convert.ToString(request.SortedProperty).ToLower() == "publishdate" && Convert.ToString(request.SortOrder).ToLower() == "descending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderByDescending(x => x.PublishDate).ToList();
                }
                if ( Convert.ToString(request.SortedProperty).ToLower() == "publishdate" && Convert.ToString(request.SortOrder).ToLower() == "ascending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderBy(x => x.PublishDate).ToList();
                }

                //filter by Uploaded By
                if ( Convert.ToString(request.SortedProperty).ToLower() == "uploadedbyuser" && Convert.ToString(request.SortOrder).ToLower() == "descending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderByDescending(x => x.UploadedByUser).ToList();
                }
                if ( Convert.ToString(request.SortedProperty).ToLower() == "uploadedbyuser" && Convert.ToString(request.SortOrder).ToLower() == "ascending" )
                {
                    lstPartnerMedia = lstPartnerMedia.OrderBy(x => x.UploadedByUser).ToList();
                }
            }
            lstPartnerMedia = lstPartnerMedia.Skip(( request.Page - 1 ) * request.PageSize).Take(request.PageSize).ToList();
            return new CollectionModel<PartnerMediaViewModel>
            {
                Items = lstPartnerMedia ,
                TotalCount = totalCount
            };
        }

        public async Task<CollectionModel<MediaAutoCompleteModel>> GetPartnerMediaAutoComplete (int partnerId , string filterString)
        {
            var partner = await _context.Partner.SingleOrDefaultAsync(x => x.Id == partnerId);
            if ( partner == null )
                throw new BusinessException("Partner not found");

            var totalCount = 0;
            var lstPartnerMedia = await _context.Media.Where(x => x.SourceId == partnerId).Select(x => new MediaAutoCompleteModel { Name = x.Name }).ToListAsync();

            if ( !string.IsNullOrEmpty(filterString) )
            {
                lstPartnerMedia = lstPartnerMedia.Where(x => x.Name.ToLower().Contains(filterString.ToLower())).ToList();
            }

            totalCount = lstPartnerMedia.Count;
            return new CollectionModel<MediaAutoCompleteModel>
            {
                Items = lstPartnerMedia ,
                TotalCount = totalCount
            };
        }

        public async Task<CollectionModel<PartnerModel>> GetShowcasePartners ()
        {
            var result = await ( from x in _context.Partner
                                 where x.ShowOnPartnerPage == true && x.IsArchived == false
                                 select new PartnerModel
                                 {
                                     Id = x.Id ,
                                     Name = x.Name ,
                                     Description = x.Description ,
                                     Logo = x.Logo ,
                                     ShowOnPartner = x.ShowOnPartnerPage ,
                                     Link = x.Link ,
                                     OrderNumber = x.OrderNumber
                                 } ).OrderBy(x => x.OrderNumber).ToListAsync();

            foreach ( var partner in result )
            {
                partner.Logo = !string.IsNullOrEmpty(partner.Logo) ? _s3BucketService.RetrieveImageCDNUrl(partner.Logo) : string.Empty;
            }

            return new CollectionModel<PartnerModel> { Items = result , TotalCount = result.Count };
        }

        public async Task<CollectionModel<PartnerDetailViewModel>> SearchPartner (FilterPartnerViewRequest partnerFilter)
        {
            IQueryable<Partner> partners;
            var isIdNumeric = partnerFilter.FilterString != null && IsNumeric(partnerFilter.FilterString);
            if ( isIdNumeric )
            {
                var id = Convert.ToInt32(partnerFilter.FilterString);
                partners = _context.Partner.Where(x => x.Id == id);
            }
            else
            {
                if ( !string.IsNullOrEmpty(partnerFilter.FilterString) )
                {
                    partners = _context.Partner.Where(x => x.Name.ToLower().Contains(partnerFilter.FilterString.ToLower()));
                }
                else
                {
                    partners = _context.Partner;
                }
            }

            var partnerQuery = ( from x in partners.AsQueryable()
                          .Include(x => x.PartnerPartnerType)
                          .ThenInclude(x => x.Partnertype)
                          select new PartnerDetailViewModel
                          {
                              Id = x.Id ,
                              Name = x.Name ,
                              PartnerType = x.PartnerPartnerType.Select(x => x.Partnertype.Name).ToList() ,
                              DateAdded = x.DateAddedUtc ,
                              IsArchived = x.IsArchived ,
                              ShowOnPartnerPage = x.ShowOnPartnerPage ,
                              Logo = x.Logo,
                          }).OrderByDescending(x => x.DateAdded); //get both items for archived and unArchived .Where(x => x.IsArchived == false)
           
            if ( !string.IsNullOrEmpty(Convert.ToString(partnerFilter.SortedProperty)) && !string.IsNullOrEmpty(Convert.ToString(partnerFilter.SortOrder)) )
            {
                if ( partnerFilter.SortedProperty.ToString().ToLower() == "name" && partnerFilter.SortOrder.ToString().ToLower() == "descending" )
                {
                    partnerQuery = partnerQuery.OrderByDescending(x => x.Name);
                }

                if ( partnerFilter.SortedProperty.ToString().ToLower() == "name" && partnerFilter.SortOrder.ToString().ToLower() == "ascending" )
                {
                    partnerQuery = partnerQuery.OrderBy(x => x.Name);
                }

                if ( partnerFilter.SortedProperty.ToString().ToLower() == "dateadded" && partnerFilter.SortOrder.ToString().ToLower() == "descending" )
                {
                    partnerQuery = partnerQuery.OrderByDescending(x => x.DateAdded);
                }

                if ( partnerFilter.SortedProperty.ToString().ToLower() == "dateadded" && partnerFilter.SortOrder.ToString().ToLower() == "ascending" )
                {
                    partnerQuery = partnerQuery.OrderBy(x => x.DateAdded);
                }
            }
            var items = await partnerQuery.ToListAsync();
            var count = items.Count();

            if ( partnerFilter.Page != 0 && partnerFilter.PageSize != 0 )
            {
                items = items.Skip((partnerFilter.Page - 1) * partnerFilter.PageSize).Take(partnerFilter.PageSize).ToList();
            }
            if ( !string.IsNullOrEmpty(partnerFilter.FilterString) )
            {
                if ( !isIdNumeric )
                {
                    items = items.Where(x => x.Name.ToLower().Contains(partnerFilter.FilterString.ToLower())).ToList();
                }
            }

            foreach ( var partner in items )
            {
                partner.Logo = !string.IsNullOrEmpty(partner.Logo) ? _s3BucketService.RetrieveImageCDNUrl(partner.Logo) : string.Empty;              
            }

            return new CollectionModel<PartnerDetailViewModel>
            {
                Items = items ,
                TotalCount = count
            };
        }

        public async Task<CollectionModel<PartnerAutoCompleteSerachViewModel>> SearchPartnersAutoComplete ()
        {
             var result = await (from x in _context.Partner
                          select new PartnerAutoCompleteSerachViewModel
                          {
                              PartnerName = x.Name,
                          }).ToListAsync();

            return new CollectionModel<PartnerAutoCompleteSerachViewModel>
            {
                Items = result,
                TotalCount = result.Count
            };
        }

        #region Private Methods
        private static bool IsNumeric (object Expression)
        {
            var isNum = double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out var retNum);
            return isNum;
        }

        private async Task<List<PartnerMediaViewModel>> PartnerMedia (int partnerId)
        {
            var partnerMedia = await (from media in _context.Media
                                      join mediaStatus in _context.MediaStatus on media.MediastatusId equals mediaStatus.Id
                                      join user in _context.User on media.UploadUserId equals user.Id
                                      where media.SourceId == partnerId && !media.IsDeleted
                                      select new PartnerMediaViewModel
                                      {
                                        Id = media.Id,
                                        Name = media.Name ,
                                        MediaStatus = mediaStatus.Name,
                                        CreatedDate = media.DateCreatedUtc,
                                        UploadedByUser = user.Name ,
                                        PublishDate = media.DatePublishedUtc,
                                        IsVisibleOnGoogle = media.IsVisibleOnGoogle
                                     }).ToListAsync();
            return partnerMedia;
        }

        #endregion
    }

    class PartnerMediaComparer : IEqualityComparer<DistributionAutocompleteViewModel>
    {
        // Media are equal if their names and product numbers are equal.
        public bool Equals (DistributionAutocompleteViewModel x , DistributionAutocompleteViewModel y)
        {

            //Check whether the compared objects reference the same data.
            if ( Object.ReferenceEquals(x , y) )
                return true;

            //Check whether any of the compared objects is null.
            if ( x is null || y is null )
                return false;

            //Check whether the products' properties are equal.
            return x.Name == y.Name;
        }

        public int GetHashCode (DistributionAutocompleteViewModel media)
        {
            //Check whether the object is null
            if ( media is null )
                return 0;

            //Calculate the hash code for the product.
            return media.Name == null ? 0 : media.Name.GetHashCode();
        }

    }
}

