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
    public class UserMediaWriteService : IUserMediaWriteService
    {

        readonly TestStoriesContext _context;
        public UserMediaWriteService (TestStoriesContext context)
        {
            _context = context;
        }
        public async Task<PlaylistModel> AddPlaylistAsync (AddPlaylistModel model)
        {
            Playlist playlist;

            if ( model.Name != null )
            {
                var isPlayListExist = _context.Playlist.Any(x => x.Name == model.Name && x.UserId == model.UserId);
                if ( isPlayListExist )
                {
                    throw new BusinessException("Playlist already exist");
                }
            }

            if ( model.UserId != 0 )
            {
                var isUserExist = _context.User.Any(x => x.Id == model.UserId);
                if ( !isUserExist )
                    throw new BusinessException("User not exist");
            }

            playlist = await AddPlaylist(new Playlist { Name = model.Name , UserId = model.UserId });

            if ( playlist == null )
            {
                throw new BusinessException("Can not add a new playlist. Please, try again.");
            }

            var response = new PlaylistModel { Id = playlist.Id , Name = playlist.Name };
            return response;
        }
        public async Task<PlaylistModel> EditPlaylistAsync (int playlistId , EditPlayListModel model)
        {
            var playlist = await _context.Playlist.SingleOrDefaultAsync(x => x.Id == playlistId);
            if ( playlist == null )
            {
                throw new BusinessException("Playlist not found");
            }

            var response = await EditPlayList(playlistId , model);
            if ( response != null )
            {
                return new PlaylistModel
                {

                    Id = response.Id ,
                    Name = response.Name
                };
            }

            throw new BusinessException("Can not edit Playlist. Please, try again.");
        }
        public async Task RemovePlaylistAsync (int playlistId)
        {
            var dbPlaylist = await _context.Playlist.SingleOrDefaultAsync(t => t.Id == playlistId);
            if ( dbPlaylist == null )
                throw new BusinessException("Playlist not found");

            await RemovePlaylist(playlistId);
        }
        public async Task<FavouriteModel> AddToFavouriteAsync (AddToFavoriteModel model)
        {
            Favorites favorite;

            var isExist = _context.Favorites.Any(x => x.UserId == model.UserId && x.MediaId == model.MediaId);
            if ( isExist )
            {
                throw new BusinessException("Media already exist in favorites");
            }
            var isUserExist = _context.User.Any(x => x.Id == model.UserId);
            if ( !isUserExist )
            {
                throw new BusinessException("User not exist");
            }

            var isMediaExist = _context.Media.Any(x => x.Id == model.MediaId);
            if ( !isMediaExist )
            {
                throw new BusinessException("Media not exist");
            }

            favorite = await AddMediaToFavourite(new Favorites { MediaId = model.MediaId , UserId = model.UserId });

            if ( favorite != null )
            {
                var addFavourite = new FavouriteModel
                {
                    FavoriteId = favorite.Id ,
                    UserId = favorite.UserId ,
                    MediaId = favorite.MediaId
                };

                return addFavourite;
            }

            throw new BusinessException("Can not add media to favourite. Please, try again.");
        }       
        public async Task<FavouriteModel> RemoveFromFavouriteAsync (AddToFavoriteModel model)
        {
            var favorite = await _context.Favorites.SingleOrDefaultAsync(x => x.MediaId == model.MediaId && x.UserId == model.UserId);
            if ( favorite == null )
            {
                throw new BusinessException("Favorite not found");
            }

            await RemoveMediaFromFavourite(new Favorites { MediaId = model.MediaId , UserId = model.UserId });

            return new FavouriteModel
            {
                FavoriteId = favorite.Id ,
                UserId = model.UserId ,
                MediaId = model.MediaId
            };
        }
        public async Task<MediaPlayListModel> AddToPlaylistAsync (AddToPlaylistModel model)
        {
            PlaylistMedia playlistMedia;

            var isPlaylistMediaExist = _context.PlaylistMedia.Where(x => x.PlaylistId == model.PlaylistId && x.MediaId == model.MediaId).Any();
            if ( isPlaylistMediaExist )
            {
                throw new BusinessException("Media already exist in playlist ");
            }

            var isPlayListExist = _context.Playlist.Any(x => x.Id == model.PlaylistId);
            if ( !isPlayListExist )
            {
                throw new BusinessException("Playlist not exist");
            }

            var mediaDetail = await _context.Media.SingleOrDefaultAsync(x => x.Id == model.MediaId);
            if ( mediaDetail is null )
            {
                throw new BusinessException("Media not exist");
            }

            if ( mediaDetail.MediatypeId != 1 && mediaDetail.MediatypeId != 2 )
            {
                throw new BusinessException("Only video/audio can be added to a playlist.");
            }

            var lastSequence = _context.PlaylistMedia.Where(x => x.PlaylistId == model.PlaylistId).OrderByDescending(x => x.MediaSequence).Select(y => y.MediaSequence).FirstOrDefault();

            playlistMedia = await AddMediaToPlaylist(new PlaylistMedia
            {
                MediaId = model.MediaId ,
                PlaylistId = model.PlaylistId ,
                MediaSequence = lastSequence + 1 ,

            });

            if ( playlistMedia != null )
            {
                var seoUrl = _context.Media.Where(x => x.Id == playlistMedia.MediaId).Select(x => x.SeoUrl).FirstOrDefault();
                var playListModel = new MediaPlayListModel
                {
                    Id = playlistMedia.Id ,
                    MediaId = playlistMedia.MediaId ,
                    PlaylistId = playlistMedia.PlaylistId ,
                    SeoUrl = seoUrl
                };

                return playListModel;
            }

            throw new BusinessException("Can not add media to playlist. Please, try again.");
        }       
        public async Task<SubscribeSeriesModel> SubscribeSeriesAsync (SubscribeSeries model)
        {
            SubscriptionSeries subscribe;

            var isUserExist = _context.User.Any(x => x.Id == model.UserId);
            if ( !isUserExist )
                throw new BusinessException("User not exist");

            var isSeriesExist = _context.Series.Where(x => x.Id == model.SeriesId).Any();
            if ( !isSeriesExist )
            {
                throw new BusinessException("Series not exist");
            }

            var isSeriesSubscribe = _context.SubscriptionSeries.Where(x => x.UserId == model.UserId && x.SeriesId == model.SeriesId).Any();
            if ( isSeriesSubscribe )
            {
                throw new BusinessException("Series already subscribed");
            }

            subscribe = await SubscribeSeries(new SubscriptionSeries
            {
                UserId = model.UserId ,
                SeriesId = model.SeriesId ,
            });

            if ( subscribe != null )
            {
                var seriesSubscription = new SubscribeSeriesModel
                {
                    SubscriptionId = subscribe.Id ,
                    SeriesId = subscribe.SeriesId ,
                    UserId = subscribe.UserId
                };

                return seriesSubscription;
            }

            throw new BusinessException("Can not subscribe Series. Please, try again.");
        }
        public async Task<SubscribeSeries> UnsubscribeSeriesAsync (SubscribeSeries model)
        {
            var subsSeries = await _context.SubscriptionSeries.SingleOrDefaultAsync(x => x.SeriesId == model.SeriesId && x.UserId == model.UserId);
            if ( subsSeries == null )
            {
                throw new BusinessException("Subscription not found");
            }

            await UnSubscribeSeries(model);
            return new SubscribeSeries
            {
                SeriesId = model.SeriesId ,
                UserId = model.UserId
            };
        }
        public async Task<SubscribeTopicModel> SubscribeTopicAsync (SubscribeTopic model)
        {
            SubscriptionTopic subscribe;

            var isUserExist = _context.User.Any(x => x.Id == model.UserId);
            if ( !isUserExist )
                throw new BusinessException("User not exist");

            var isTopicExist = _context.Topic.Where(x => x.Id == model.TopicId).Any();
            if ( !isTopicExist )
            {
                throw new BusinessException("Topic not exist");
            }

            var isTopicSubscribe = _context.SubscriptionTopic.Where(x => x.UserId == model.UserId && x.TopicId == model.TopicId).Any();

            if ( isTopicSubscribe )
            {
                throw new BusinessException("Topic already subscribed");
            }

            subscribe = await SubscribeTopic(new SubscriptionTopic
            {
                UserId = model.UserId ,
                TopicId = model.TopicId
            });

            if ( subscribe != null )
            {
                var subscribeTopic = new SubscribeTopicModel
                {
                    SubscriptionId = subscribe.Id ,
                    UserId = subscribe.UserId ,
                    TopicId = subscribe.TopicId
                };

                return subscribeTopic;
            }

            throw new BusinessException("Can not subscribe Topic. Please, try again.");
        }
        public async Task<SubscribeTopic> UnsubscribeTopicAsync (SubscribeTopic model)
        {
            var subTopic = await _context.SubscriptionTopic.SingleOrDefaultAsync(x => x.UserId == model.UserId && x.TopicId == model.TopicId);
            if ( subTopic == null )
            {
                throw new BusinessException("Subscrition not found");
            }

            await UnSubscribeTopic(model);
            return new SubscribeTopic
            {
                UserId = model.UserId ,
                TopicId = model.TopicId
            };
        }      
        public async Task<AddWatchHistory> AddWatchHistoryAsync (long mediaId , int userId)
        {
            if ( userId != 0 )
            {
                var isUserExist = _context.User.Any(x => x.Id == userId);
                if ( !isUserExist )
                    throw new BusinessException("User not exist");
            }

            var isMediaExist = _context.Media.Any(x => x.Id == mediaId);
            if ( !isMediaExist )
            {
                throw new BusinessException("Media not exist");
            }
            var watchedEntity = new WatchHistory
            {
                MediaId = mediaId ,
                UserId = userId ,
                LastWatchedUtc = DateTime.UtcNow
            };

            var watchHistoryItem = await AddWatchHistory(watchedEntity);
            return new AddWatchHistory
            {
                UserId = watchHistoryItem.UserId ,
                MediaId = watchHistoryItem.MediaId ,
                CreatedDate = watchHistoryItem.LastWatchedUtc
            };
        }
        public async Task RemoveWatchHistoryAsync (long mediaId, int userId)
        {
            if ( userId != 0 )
            {
                var isUserExist = _context.User.Any(x => x.Id == userId);
                if ( !isUserExist )
                    throw new BusinessException("User not exist");
            }

            var isMediaExist = _context.Media.Any(x => x.Id == mediaId);
            if ( !isMediaExist )
            {
                throw new BusinessException("Media not exist");
            }

            await RemoveWatchHistory(mediaId , userId);
        }

        #region Private Methods
        private async Task<Playlist> AddPlaylist (Playlist entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        private async Task<Favorites> AddMediaToFavourite (Favorites entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        private async Task RemoveMediaFromFavourite (Favorites entity)
        {
            var favorite = await _context.Favorites.SingleOrDefaultAsync(x => x.MediaId == entity.MediaId && x.UserId == entity.UserId);
            if ( favorite != null )
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }
        private async Task<PlaylistMedia> AddMediaToPlaylist (PlaylistMedia entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        private async Task<Playlist> EditPlayList (int playlistId , EditPlayListModel entity)
        {
            using ( var transaction = _context.Database.BeginTransaction() )
            {
                try
                {
                    var dbPlaylist = await _context.Playlist.Include(x => x.PlaylistMedia).SingleOrDefaultAsync(t => t.Id == playlistId);
                    if ( dbPlaylist != null )
                    {
                        dbPlaylist.Name = entity.PlaylistName;
                        _context.Playlist.Update(dbPlaylist);
                        _context.RemoveRange(dbPlaylist.PlaylistMedia);
                        if ( entity.UpdatedMediaIds != null )
                        {
                            var updatedMediaItems = new List<PlaylistMedia>();
                            for ( var i = 0 ;i < entity.UpdatedMediaIds.Count ;i++ )
                            {
                                var isMediaExist = _context.Media.Where(x => x.Id == entity.UpdatedMediaIds[i]).Any();
                                if ( isMediaExist )
                                {
                                    updatedMediaItems.Add(new PlaylistMedia { PlaylistId = playlistId , MediaId = entity.UpdatedMediaIds[i] , MediaSequence = i + 1 });
                                }
                            }
                            if ( updatedMediaItems != null )
                            {
                                _context.PlaylistMedia.AddRange(updatedMediaItems);
                            }
                        }
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        return dbPlaylist;
                    }
                }
                catch
                {
                    transaction.Rollback();
                }
            }
            return null;
        }
        private async Task RemovePlaylist (int playlistId)
        {
            using ( var transaction = _context.Database.BeginTransaction() )
            {
                try
                {
                    var playlist = await _context.Playlist.SingleOrDefaultAsync(x => x.Id == playlistId);
                    if ( playlist != null )
                    {
                        _context.Playlist.Remove(playlist);
                    }
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch ( Exception )
                {
                    transaction.Rollback();
                }
            }
        }
        private async Task<SubscriptionSeries> SubscribeSeries (SubscriptionSeries entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        private async Task UnSubscribeSeries (SubscribeSeries entity)
        {
            var subSeries = await _context.SubscriptionSeries.SingleOrDefaultAsync(x => x.SeriesId == entity.SeriesId && x.UserId == entity.UserId);
            if ( subSeries != null )
            {
                _context.SubscriptionSeries.Remove(subSeries);
                await _context.SaveChangesAsync();
            }
        }
        private async Task<SubscriptionTopic> SubscribeTopic (SubscriptionTopic entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        private async Task UnSubscribeTopic (SubscribeTopic entity)
        {
            var subTopic = await _context.SubscriptionTopic.SingleOrDefaultAsync(t => t.UserId == entity.UserId && t.TopicId == entity.TopicId);
            if ( subTopic != null )
            {
                _context.SubscriptionTopic.Remove(subTopic);
                await _context.SaveChangesAsync();
            }
        }
        private async Task<WatchHistory> AddWatchHistory (WatchHistory entity)
        {
            var watchHistory = await _context.WatchHistory.SingleOrDefaultAsync(x => x.UserId == entity.UserId && x.MediaId == entity.MediaId);
            if ( watchHistory != null )
            {
                watchHistory.LastWatchedUtc = entity.LastWatchedUtc;
                _context.Update(watchHistory);
            }
            else
            {
                await _context.AddAsync(entity);
            }
            await _context.SaveChangesAsync();
            return entity;
        }
        private async Task RemoveWatchHistory (long mediaId , int userId)
        {
            var watchHistory = await _context.WatchHistory.SingleOrDefaultAsync(t => t.MediaId == mediaId && t.UserId == userId);
            if ( watchHistory != null )
            {
                _context.WatchHistory.Remove(watchHistory);
                await _context.SaveChangesAsync();
            }
        }

        #endregion
    }
}
