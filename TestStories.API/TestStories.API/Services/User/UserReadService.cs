using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Services
{
    public class UserReadService : IUserReadService
    {
        private readonly TestStoriesContext _context;
        private const string DefaultCompanyName = "Singleton Foundation";
        readonly IS3BucketService _s3BucketService;
        public UserReadService (TestStoriesContext context, IS3BucketService s3BucketService)
        {
            _context = context;
            _s3BucketService = s3BucketService;
        }

        private IQueryable<User> Users => _context.User;

        private IQueryable<UserType> UserTypes => _context.UserType;

        private IQueryable<Partner> Partners => _context.Partner;

        private IQueryable<Media> Medias => _context.Media;

        private IQueryable<PlaylistMedia> PlaylistMedia => _context.PlaylistMedia;

        private IQueryable<Favorites> Favorites => _context.Favorites;

        public async Task<CollectionModel<ShortUserModel>> GetShortUsers (FilterUserRequest filter)
        {
            var query = from user in Users
                          .Include(x => x.Usertype)
                          .Include(x => x.Userstatus)
                          join partner in Partners
                          on user.PartnerId equals partner.Id into userGroup
                          from userDetail in userGroup.DefaultIfEmpty()
                          select new ShortUserModel()
                          {
                              Id = user.Id ,
                              FirstName = user.FirstName ,
                              LastName = user.LastName ,
                              UserType = user.Usertype.Name ,
                              Company = userDetail.Name ,
                              Status = user.Userstatus.Name ,
                              DateAdded = user.DateCreatedUtc ,
                              Email = user.Email
                          };

            if ( !string.IsNullOrEmpty(filter.FilterString))
            { 
                query = query.Where(x => x.FirstName.Contains(filter.FilterString) || x.LastName.Contains(filter.FilterString) || filter.FilterString.Contains(x.FirstName) || filter.FilterString.Contains(x.LastName));
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(x => x.DateAdded.Date >= filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue )
            {
                query = query.Where(x => x.DateAdded.Date <= filter.ToDate.Value.Date);
            }

            if ( !string.IsNullOrEmpty(filter.UserType) )
            {

                query = query.Where(x => x.UserType == filter.UserType);
            }

            if ( !string.IsNullOrEmpty(filter.UserStatus) )
            {

                query = query.Where(x => x.Status == filter.UserStatus);
            }

            if ( !string.IsNullOrEmpty(filter.Company) )
            {

                query = query.Where(x => x.Company == filter.Company);
            }

            if ( !string.IsNullOrEmpty(filter.SortedProperty) && !string.IsNullOrEmpty(filter.SortOrder) )
            {
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "dateadded" && Convert.ToString(filter.SortOrder).ToLower() == "descending" )
                {
                    query = query.OrderByDescending(x => x.DateAdded);
                }
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "dateadded" && Convert.ToString(filter.SortOrder).ToLower() == "ascending" )
                {
                    query = query.OrderBy(x => x.DateAdded);
                }
                //usertype
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "usertype" && Convert.ToString(filter.SortOrder).ToLower() == "descending" )
                {
                    query = query.OrderByDescending(x => x.UserType);
                }
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "usertype" && Convert.ToString(filter.SortOrder).ToLower() == "ascending" )
                {
                    query = query.OrderBy(x => x.UserType);
                }
                //status
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "status" && Convert.ToString(filter.SortOrder).ToLower() == "descending" )
                {
                    query = query.OrderByDescending(x => x.Status);
                }
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "status" && Convert.ToString(filter.SortOrder).ToLower() == "ascending" )
                {
                    query = query.OrderBy(x => x.Status);
                }
                //partner
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "partner" && Convert.ToString(filter.SortOrder).ToLower() == "descending" )
                {
                    query = query.OrderByDescending(x => x.Company);
                }
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "partner" && Convert.ToString(filter.SortOrder).ToLower() == "ascending" )
                {
                    query = query.OrderBy(x => x.Company);
                }
                //lastName
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "lastname" && Convert.ToString(filter.SortOrder).ToLower() == "descending" )
                {
                    query = query.OrderByDescending(x => x.LastName);
                }
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "lastname" && Convert.ToString(filter.SortOrder).ToLower() == "ascending" )
                {
                    query = query.OrderBy(x => x.LastName);
                }
                //firsnname
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "firstname" && Convert.ToString(filter.SortOrder).ToLower() == "descending" )
                {
                    query = query.OrderByDescending(x => x.FirstName);
                }
                if ( Convert.ToString(filter.SortedProperty).ToLower() == "firstname" && Convert.ToString(filter.SortOrder).ToLower() == "ascending" )
                {
                    query = query.OrderBy(x => x.FirstName);
                }
            }

            var totalCount = await query.CountAsync();

            var users = await query.Skip(( filter.Page - 1 ) * filter.PageSize).Take(filter.PageSize).ToListAsync();

            return new CollectionModel<ShortUserModel>
            {
                Items = users ,
                TotalCount = totalCount,
                PageSize = filter.PageSize,
                PageNumber = filter.Page
            };
        }

        public async Task<CollectionModel<UserAutoCompleteSearch>> UserAutoCompleteSearch ()
        {
            var userNames = ( from user in Users
                              select new UserAutoCompleteSearch
                              {
                                  Name = user.FirstName + " " + user.LastName ,
                              } ).ToList();
            userNames = userNames.Where(x => x.Name != null).ToList();

            var companyNames = ( from user in Users
                                 join source in Partners on user.PartnerId equals source.Id
                                 select new UserAutoCompleteSearch
                                 {
                                     Name = source.Name
                                 } ).ToList();
            companyNames = companyNames.Where(x => x.Name != null).ToList();
            var users = userNames.Union(companyNames , new UserComparer()).OrderBy(x => x.Name).ToList();

            var totalCount = users.Count;
            //var users = await query.ToListAsync();
            return new CollectionModel<UserAutoCompleteSearch>
            {
                Items = users ,
                TotalCount = totalCount
            };
        }

        public async Task<playListItem> GetUserPlayListMedia (int userId)
        {
            var isUserExist = _context.User.Any(x => x.Id == userId);
            if (!isUserExist) 
                throw new BusinessException("User not exist");

            var playlists = ( from playlistItem in UserPlaylist(userId)
                              let allMedia = ( from plmedia in PlaylistMedia
                                               join media in Medias on plmedia.MediaId equals media.Id
                                               where plmedia.PlaylistId == playlistItem.Id && media.MediastatusId != (int)MediaStatusEnum.Archived
                                               && media.MediastatusId != (int)MediaStatusEnum.Draft && !media.IsDeleted
                                               join serie in _context.Series on plmedia.Media.SeriesId equals serie.Id into srGroup
                                               from series in srGroup.DefaultIfEmpty()
                                               select new MediaInfoModel
                                               {
                                                   Id = media.Id ,
                                                   Title = media.Name ,
                                                   Description = media.Description ,
                                                   Url = media.Url ,
                                                   FeaturedImage = media.FeaturedImage ,
                                                   Thumbnail = media.Thumbnail ,
                                                   MediaTypeId = media.MediatypeId ,
                                                   SeoUrl = media.SeoUrl ,
                                                   UniqueId = media.UniqueId ,
                                                   IsSharingAllowed = media.IsSharingAllowed,
                                                   SeriesId = series.Id,
                                                   Series =  series.Name
                                               } ).ToList()
                              select new UserPlayListViewModel
                              {
                                  Id = playlistItem.Id ,
                                  Name = playlistItem.Name ,
                                  Medias = allMedia
                              } ).ToList();
            foreach ( var playlist in playlists )
            {
                playlist.Medias = playlist.Medias.Where(media => IsMediaActive(media.Id)).ToList();
                var mediaThumbnail = playlist.Medias.Count > 0 ? playlist.Medias.FirstOrDefault().FeaturedImage : "";
                playlist.MediaThumbnail = !string.IsNullOrEmpty(mediaThumbnail) ? _s3BucketService.RetrieveImageCDNUrl(mediaThumbnail) : string.Empty;
                playlist.MediaThumbnails = await _s3BucketService.GetCompressedImages(mediaThumbnail , EntityType.Media);
                foreach ( var media in playlist.Medias )
                {
                    var featuredImage = media.FeaturedImage;
                    media.FeaturedImage = !string.IsNullOrEmpty(media.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(media.FeaturedImage) : string.Empty;
                    media.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage , EntityType.Media);
                    var thumbnail = media.Thumbnail;
                    media.Thumbnail = !string.IsNullOrEmpty(media.Thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(media.Thumbnail) : string.Empty;
                    media.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail , EntityType.Media);
                }
            }
            return new playListItem
            {
                Playlists = playlists ,
                PlaylistCount = playlists.Count
            };
        }

        public async Task<List<UserSubscriptionModel>> GetUserSubscriptions (int userId)
        {
            var isUserExist = _context.User.Any(x => x.Id == userId);
            if (!isUserExist)
                throw new BusinessException("User not exist");

            var lstSubscribedSeries = ( from Subsr in _context.SubscriptionSeries
                                        join sr in _context.Series on Subsr.SeriesId equals sr.Id
                                        where Subsr.UserId == userId
                                        select new UserSubscriptionModel
                                        {
                                            Id = sr.Id ,
                                            Name = sr.Name ,
                                            Type = "Series" ,
                                            SeoUrl = sr.SeoUrl
                                        } );
            var lstSubscribedTopics = ( from Subtp in _context.SubscriptionTopic
                                        join tp in _context.Topic on Subtp.TopicId equals tp.Id
                                        where Subtp.UserId == userId
                                        select new UserSubscriptionModel
                                        {
                                            Id = tp.Id ,
                                            Name = tp.Name ,
                                            Type = "Topic" ,
                                            SeoUrl = tp.SeoUrl
                                        } );
            var userSubscriptions = ( lstSubscribedSeries.Union(lstSubscribedTopics) ).OrderBy(x => x.Name).ToList();
            return userSubscriptions;
        }

        public async Task<List<FavouriteUserModel>> GetUserFavorites (int userId)
        {
            var isUserExist = _context.User.Any(x => x.Id == userId);
            if (!isUserExist) 
                throw new BusinessException("User not exist");

            var query =  await ( from favorite in Favorites.Include(x => x.Media)
                          where favorite.UserId == userId && favorite.Media.MediastatusId != (int)MediaStatusEnum.Archived && !favorite.Media.IsDeleted
                          join serie in _context.Series on favorite.Media.SeriesId equals serie.Id into srGroup
                          from series in srGroup.DefaultIfEmpty()
                          select new FavouriteUserModel
                          {
                              FavoriteId = favorite.Id ,
                              MediaId = favorite.Media.Id ,
                              MediaName = favorite.Media.Name ,
                              Description = favorite.Media.Description ,
                              Thumbnail = favorite.Media.Thumbnail ,
                              FeaturedImage = favorite.Media.FeaturedImage ,
                              Url = favorite.Media.Url ,
                              MediaTypeId = favorite.Media.MediatypeId ,
                              IsSharingAllowed = favorite.Media.IsSharingAllowed ,
                              SeoUrl = favorite.Media.SeoUrl,
                              UniqueId = favorite.Media.UniqueId,
                              SeriesId = series.Id,
                              Series = series.Name
                          }).ToListAsync();
            var result =  query.Where(media => IsMediaActive(media.MediaId)).ToList();
            foreach ( var favorite in result )
            {
                var featuredImage = favorite.FeaturedImage;
                favorite.FeaturedImage = !string.IsNullOrEmpty(favorite.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(favorite.FeaturedImage) : string.Empty;
                favorite.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage , EntityType.Media);
                var thumbnail = favorite.Thumbnail;
                favorite.Thumbnail = !string.IsNullOrEmpty(favorite.Thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(favorite.Thumbnail) : string.Empty;
                favorite.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail , EntityType.Media);
            }
            return result;
        }

        public List<Playlist> UserPlaylist (int userId)
        {
            return _context.Playlist.Where(x => x.UserId == userId).ToList();
        }

        public async Task<(int? AdminUserTypeId, int? AdminEditorUserTypeId, int? PartnerUserTypeId)> GetAdminPartnerTypeIdsAsync ()
        {

            const string adminUserTypeName = "Admin";
            const string adminEditorUserTypeName = "Admin-Editor";
            const string partnerUserTypeName = "Partner-User";

            var result = (AdminUserTypeId: default(int?), AdminEditorUserTypeId: default(int?),
                PartnerUserTypeId: default(int?));

            var adminUserType = await UserTypes.SingleOrDefaultAsync(u => u.Name == adminUserTypeName);
            if ( adminUserType != null )
            {
                result.AdminUserTypeId = adminUserType.Id;
            }

            var adminEditorUserType =
                await UserTypes.SingleOrDefaultAsync(u => u.Name == adminEditorUserTypeName);
            if ( adminEditorUserType != null )
            {
                result.AdminEditorUserTypeId = adminEditorUserType.Id;
            }

            var partnerUserType = await UserTypes.SingleOrDefaultAsync(u => u.Name == partnerUserTypeName);
            if ( partnerUserType != null )
            {
                result.PartnerUserTypeId = partnerUserType.Id;
            }

            return result;

        }

        public async Task<ShortUserModel> GetUserById(int id)
        {
            var query = await CreateShortUserModelQuery();
            var result = query.SingleOrDefault(u => u.Id == id);
            return result;
        }

        public async Task<int> GetUserIdByEmail (string email)
        {
            var userId = 0;
            if ( !string.IsNullOrEmpty(email))
            {
                var user = await _context.User.FirstOrDefaultAsync(x => x.Email == email);
                if ( user !=null )
                {
                    userId = user.Id;
                }
            }
            return userId;
        }

        public async Task<UserData> GetUserSubscriptionItems(int userId)
        {
            var isUserExist = _context.User.Any(x => x.Id == userId);
            if ( !isUserExist )
                throw new BusinessException("User not exist");

            var userPlaylists = GetUserPlayListsAsync(userId);
            var userFavorites = GetUserFavoritesAsync(userId);
            var userSubscriptions = GetUserSubscriptionsAsync(userId);
            var userWatchHistory = GetUserWatchHistoryAsync(userId);

            await Task.WhenAll(userPlaylists , userFavorites , userSubscriptions , userWatchHistory);
            return new UserData
            {
                Playlists = userPlaylists.Result,
                Favorites = userFavorites.Result,
                Subscriptions = userSubscriptions.Result,
                WatchHistory = userWatchHistory.Result,
            };
        }


        public async Task<string> GetApiKeyByEmail(string email, ClaimsPrincipal claimsPrincipal)
        {
            var roles = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? UserTypeEnum.User.ToString();
            if (!roles.Contains("SuperAdmin"))
            {
                throw new BusinessException("Invalid User");
            }

            var apiKey = String.Empty;
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _context.User.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    apiKey = user.ApiKey;
                }
            }
            return apiKey;
        }

        public async Task<bool> ValidateApiKey(string apiKey)
        {
            if(String.IsNullOrEmpty(apiKey))
            {
                return false;
            }
            var userWithApiKey = await _context.User.FirstAsync(c => c.ApiKey == apiKey);
            return userWithApiKey != null && userWithApiKey.ApiKey == apiKey;
        }


        #region Private Methods

        /// <summary>
        /// Check media status
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        private bool IsMediaActive (long mediaId)
        {
            var media = Medias.FirstOrDefault(x => x.Id == mediaId);

            if ( media != null )
            {
                if ( media.ActiveFromUtc != null && media.ActiveToUtc != null )
                    return media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc;
                else if ( media.ActiveFromUtc != null && media.ActiveToUtc == null )
                    return media.ActiveFromUtc <= DateTime.UtcNow;
                else if ( media.ActiveFromUtc == null && media.ActiveToUtc != null )
                    return DateTime.UtcNow <= media.ActiveToUtc;
                else
                    return true;
            }
            return false;
        }
      
        private async Task<IQueryable<ShortUserModel>> CreateShortUserModelQuery()
        {
            var (adminUserTypeId, adminEditorUserTypeId, partnerUserTypeId) = await GetAdminPartnerTypeIdsAsync();

            var partnerQuery = _context.Partner.Select(x => new { partnerId = x.Id, partnerName = x.Name });

            return _context.User.Include(x => x.Userstatus).Include(u => u.Usertype).Select(item =>
                new ShortUserModel
                {
                    Id = item.Id,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Email = item.Email,
                    UserType = item.Usertype != null ? item.Usertype.Name : string.Empty,
                    UserTypeId = item.UsertypeId,
                    Company = item.UsertypeId == adminUserTypeId || item.UsertypeId == adminEditorUserTypeId
                        ? DefaultCompanyName
                        : item.UsertypeId == partnerUserTypeId && item.PartnerId.HasValue
                            ? partnerQuery.Where(p => p.partnerId == item.PartnerId.Value).Select(p => p.partnerName).First()
                            : string.Empty,
                    PartnerId = item.PartnerId,
                    DateAdded = item.DateCreatedUtc,
                    Status = item.Userstatus == null ? string.Empty : item.Userstatus.Name,
                    Phone = item.Phone ?? string.Empty,
                    IsNewsletterSubscribed = item.IsNewsletterSubscribed,
                    ApiKey = item.UsertypeId == (byte)UserTypeEnum.SuperAdmin ? item.ApiKey : String.Empty
                });
        }

        private async Task<List<UserPlayListsModel>> GetUserPlayListsAsync (int userId)
        {
            var playlists = ( from playlistItem in UserPlaylist(userId)
                              let allMedia = ( from plmedia in PlaylistMedia
                                               join media in Medias on plmedia.MediaId equals media.Id
                                               where plmedia.PlaylistId == playlistItem.Id && media.MediastatusId != (int)MediaStatusEnum.Archived
                                               && media.MediastatusId != (int)MediaStatusEnum.Draft && !media.IsDeleted
                                               select new UserPlaylistMedia
                                               {
                                                   Id = media.Id,
                                                   FeaturedImage = media.FeaturedImage,
                                                   Thumbnail = media.Thumbnail,
                                                   SeoUrl = media.SeoUrl,
                                                   UniqueId = media.UniqueId 
                                               } ).ToList()
                              select new UserPlayListsModel
                              {
                                  Id = playlistItem.Id,
                                  Name = playlistItem.Name,
                                  Medias = allMedia
                              } ).ToList();
            foreach ( var playlist in playlists )
            {
                playlist.Medias = playlist.Medias.Where(media => IsMediaActive(media.Id)).ToList();
                var mediaThumbnail = playlist.Medias.Count > 0 ? playlist.Medias.FirstOrDefault().FeaturedImage : "";
                playlist.MediaThumbnail = !string.IsNullOrEmpty(mediaThumbnail) ? ( await _s3BucketService.GetCompressedImages(mediaThumbnail , EntityType.Media) ).Grid : string.Empty;
            
                await Task.Run(() =>
                {
                    Parallel.ForEach(playlist.Medias , async media =>
                    {
                        media.FeaturedImage =  (await _s3BucketService.GetCompressedImages(media.FeaturedImage , EntityType.Media)).Grid;
                        media.Thumbnail = (await _s3BucketService.GetCompressedImages(media.Thumbnail , EntityType.Media)).Grid;
                    });
                });
            }
            return playlists;
        }

        private async Task<List<UserFavoritesModel>> GetUserFavoritesAsync (int userId)
        {
            var favorites = await ( from favorite in Favorites.Include(x => x.Media)
                                where favorite.UserId == userId && favorite.Media.MediastatusId != (int)MediaStatusEnum.Archived && !favorite.Media.IsDeleted
                                 && ( favorite.Media.ActiveFromUtc.HasValue &&  favorite.Media.ActiveToUtc.HasValue ? favorite.Media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= favorite.Media.ActiveToUtc :
                                          favorite.Media.ActiveFromUtc.HasValue && !favorite.Media.ActiveToUtc.HasValue ? favorite.Media.ActiveFromUtc <= DateTime.UtcNow :
                                         favorite.Media.ActiveFromUtc.HasValue || !favorite.Media.ActiveToUtc.HasValue || DateTime.UtcNow <= favorite.Media.ActiveToUtc )
                                    select new UserFavoritesModel
                                {
                                    Id = favorite.Id,
                                    MediaId = favorite.Media.Id,
                                    MediaName = favorite.Media.Name,
                                    Thumbnail = favorite.Media.Thumbnail,
                                    FeaturedImage = favorite.Media.FeaturedImage,
                                    SeoUrl = favorite.Media.SeoUrl,
                                    UniqueId = favorite.Media.UniqueId,
                                } ).ToListAsync();

            await Task.Run(() =>
                {
                    Parallel.ForEach(favorites , async media =>
                    {
                        media.FeaturedImage =  (await _s3BucketService.GetCompressedImages(media.FeaturedImage , EntityType.Media)).Grid;
                        media.Thumbnail = (await _s3BucketService.GetCompressedImages(media.Thumbnail , EntityType.Media)).Grid;
                    });
                });

            return favorites;
        }

        private async Task<List<UserSubscriptionModel>> GetUserSubscriptionsAsync (int userId)
        {
            var lstSubscribedSeries = ( from Subsr in _context.SubscriptionSeries
                                        join sr in _context.Series on Subsr.SeriesId equals sr.Id
                                        where Subsr.UserId == userId
                                        select new UserSubscriptionModel
                                        {
                                            Id = sr.Id,
                                            Name = sr.Name,
                                            Type = "Series",
                                            SeoUrl = sr.SeoUrl
                                        } );
            var lstSubscribedTopics = ( from Subtp in _context.SubscriptionTopic
                                        join tp in _context.Topic on Subtp.TopicId equals tp.Id
                                        where Subtp.UserId == userId
                                        select new UserSubscriptionModel
                                        {
                                            Id = tp.Id,
                                            Name = tp.Name,
                                            Type = "Topic",
                                            SeoUrl = tp.SeoUrl
                                        } );
            return await (lstSubscribedSeries.Union(lstSubscribedTopics)).OrderBy(x => x.Name).ToListAsync();           
        }

        private async Task<List<UserWatchHistoryModel>> GetUserWatchHistoryAsync (int userId)
        {
            var watchHistory = await ( from watch in _context.WatchHistory.Include(x => x.Media).ThenInclude(y => y.Series).OrderByDescending(x => x.LastWatchedUtc)
                           where watch.UserId == userId && watch.Media.MediastatusId != (int)MediaStatusEnum.Archived
                           && !watch.Media.IsDeleted
                           && ( watch.Media.ActiveFromUtc.HasValue &&  watch.Media.ActiveToUtc.HasValue ? watch.Media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= watch.Media.ActiveToUtc :
                                          watch.Media.ActiveFromUtc.HasValue && !watch.Media.ActiveToUtc.HasValue ? watch.Media.ActiveFromUtc <= DateTime.UtcNow :
                                         watch.Media.ActiveFromUtc.HasValue || !watch.Media.ActiveToUtc.HasValue || DateTime.UtcNow <= watch.Media.ActiveToUtc )
                           select new UserWatchHistoryModel
                           {
                               Id = watch.Id,
                               MediaId = watch.Media.Id,
                               MediaName = watch.Media.Name,
                               Thumbnail = watch.Media.Thumbnail,
                               FeaturedImage = watch.Media.FeaturedImage,
                               SeoUrl = watch.Media.SeoUrl,
                               UniqueId = watch.Media.UniqueId
                           } ).ToListAsync();

            await Task.Run(() =>
            {
                Parallel.ForEach(watchHistory , async media =>
                {
                    media.FeaturedImage =  ( await _s3BucketService.GetCompressedImages(media.FeaturedImage , EntityType.Media) ).Grid;
                    media.Thumbnail = ( await _s3BucketService.GetCompressedImages(media.Thumbnail , EntityType.Media) ).Grid;
                });
            });

            return watchHistory; 
        }

        #endregion
        class UserComparer : IEqualityComparer<UserAutoCompleteSearch>
        {
            // User are equal if their names and product numbers are equal.
            public bool Equals (UserAutoCompleteSearch x , UserAutoCompleteSearch y)
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

            public int GetHashCode (UserAutoCompleteSearch user)
            {
                //Check whether the object is null
                if ( user is null )
                    return 0;

                //Calculate the hash code for the product.
                return user.Name == null ? 0 : user.Name.GetHashCode();
            }

        }
    }
}
