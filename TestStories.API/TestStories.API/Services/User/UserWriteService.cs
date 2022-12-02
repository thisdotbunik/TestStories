using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.Common.Configurations;
using TestStories.Common.Events;
using TestStories.Common.MailKit;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;
using Microsoft.Extensions.Logging;
using io.fusionauth;
using io.fusionauth.domain.api;
using io.fusionauth.domain.api.user;
using io.fusionauth.domain;
using User = TestStories.DataAccess.Entities.User;
using FusionUser = io.fusionauth.domain.User;
using System.Web;

namespace TestStories.API.Concrete
{
    /// <inheritdoc />
    public class UserWriteService : IUserWriteService
    {
        readonly TestStoriesContext _context;
        readonly EmailSettings _emailSettings;
        readonly AppSettings _appSettings;
        private readonly ILogger<UserWriteService> _logger;
        readonly IPublishEvent<SendEmail> _eventPublisher;
        const string DefaultCompanyName = "Singleton Foundation";

        /// <inheritdoc />
        public UserWriteService (TestStoriesContext context , IOptions<AppSettings> appSettings , ILogger<UserWriteService> logger ,
            IOptions<EmailSettings> emailSettings ,
            IPublishEvent<SendEmail> eventPublisher )
        {
            _context = context;
            _emailSettings = emailSettings.Value;
            _appSettings = appSettings.Value;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        private IQueryable<User> Users => _context.User;

        private IQueryable<UserType> UserTypes => _context.UserType;

        private IQueryable<UserStatus> UserStatuses => _context.UserStatus;

        public async Task<ShortUserModel> CreateUserAsync (AddUserModel request , ClaimsPrincipal claimsPrincipal)
        {
            if ( request.Email != null )
            {
                var isMailExist = Users.Where(x => x.Email == request.Email).Any();
                if ( isMailExist )
                {
                    throw new BusinessException("Email already exist");
                }
            }
            var userEntity = new User
            {
                FirstName = request.FirstName ,
                LastName = request.LastName ,
                Email = request.Email ,
                UsertypeId = request.UserTypeId ,
                Password = string.Empty ,
                DateCreatedUtc = DateTime.UtcNow
            };

            if ( !string.IsNullOrEmpty(request.Phone) )
            {
                userEntity.Phone = request.Phone;
            }

            var companyName = string.Empty;
            var (adminUserTypeId, adminEditorUserTypeId, partnerUserTypeId) = await GetAdminPartnerTypeIdsAsync();
            if ( request.UserTypeId == adminUserTypeId || request.UserTypeId == adminEditorUserTypeId )
            {
                var parent = await _context.Partner.SingleOrDefaultAsync(x => x.Name == DefaultCompanyName);
                if ( parent != null )
                {
                    userEntity.PartnerId = parent.Id;
                    companyName = parent.Name;
                }
            }
            if ( request.UserTypeId == partnerUserTypeId )
            {
                if ( request.PartnerId.HasValue && request.PartnerId.Value > 0 )
                {
                    var parent = await _context.Partner.SingleOrDefaultAsync(x => x.Id == request.PartnerId.Value);
                    if ( parent != null )
                    {
                        userEntity.PartnerId = parent.Id;
                        companyName = parent.Name;
                    }
                }
                else
                {
                    throw new BusinessException("The Partner can not be empty.");
                }
            }

            var userStatusName = string.Empty;
            var userStatus = await UserStatuses.SingleOrDefaultAsync(x => x.Name.Trim() == UserStatusEnum.Invited.ToString());
            if ( userStatus != null )
            {
                userEntity.UserstatusId = userStatus.Id;
                userStatusName = userStatus.Name;
            }

            var userTypeName = string.Empty;
            var userType = await UserTypes.SingleOrDefaultAsync(x => x.Id == request.UserTypeId);
            if ( userType != null )
            {
                userTypeName = userType.Name;
            }


            var user = await AddUserAsync(userEntity);
            if ( user != null )
            {
                // Get Inviting User Details
                var invitingUserEmail = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var invitingUserFName = string.Empty;
                var invitingUserLName = string.Empty;
                if ( !string.IsNullOrEmpty(invitingUserEmail) )
                {
                    var invitingUser = await Users.SingleOrDefaultAsync(x => x.Email == invitingUserEmail);
                    if ( invitingUser != null )
                    {
                        invitingUserFName = invitingUser.FirstName;
                        invitingUserLName = invitingUser.LastName;
                    }
                }

                var token = await GeneratResetPasswordToken(user);
                await SendSetPasswordMail(user.Email , user.FirstName , user.LastName , invitingUserFName , invitingUserLName , token);
                var result = new ShortUserModel
                {
                    Id = user.Id ,
                    FirstName = user.FirstName ,
                    LastName = user.LastName ,
                    UserType = userTypeName ,
                    Company = companyName ,
                    DateAdded = user.DateCreatedUtc ,
                    Status = userStatusName
                };
                return result;
            }

            throw new BusinessException("Can not add a new user. Please, try again.");
        }

        public async Task<ShortUserModel> EditUserAsync (EditUserModel entity , ClaimsPrincipal claimsPrincipal)
        {
            if ( entity.Email != null )
            {
                var dbUser = await _context.User.SingleOrDefaultAsync(x => x.Email.Trim() == entity.Email.Trim());
                if ( dbUser != null )
                    if ( dbUser.Id != entity.Id )
                    {
                        throw new BusinessException("Email already exist");
                    }
            }

            var roles = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? UserTypeEnum.User.ToString();
            if ( !roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            {
                var userEmail = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if ( userEmail != entity.Email )
                {
                    throw new BusinessException("Invalid User");
                }
            }

            var userDetail = await _context.User.SingleOrDefaultAsync(x => x.Id == entity.Id);          

            if ( !roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            {
                entity.Email = userDetail.Email;
            }

            var userEntity = new User
            {
                Id = entity.Id ,
                FirstName = entity.FirstName ,
                LastName = entity.LastName ,
                Email = entity.Email ,
                UsertypeId = entity.UserTypeId ,
                PartnerId = userDetail.PartnerId
            };

            if ( !string.IsNullOrEmpty(entity.Phone) )
            {
                userEntity.Phone = entity.Phone;
            }

            if(roles.Contains("SuperAdmin"))
            {
                userEntity.ApiKey = entity.ApiKey;
            }

            var user = await UpdateUserAsync(userEntity);
            if ( user != null )
            {
                return new ShortUserModel
                {
                    Id = user.Id ,
                    FirstName = user.FirstName ,
                    LastName = user.LastName ,
                    Email = user.Email ,
                    Phone = user.Phone ,
                    UserTypeId = user.UsertypeId,
                    ApiKey = user.ApiKey
                };
            }
            throw new BusinessException("Can not edit a user. Please, try again.");
        }
        private async Task<User> UpdateUserAsync (User entity)
        {
            var userDetail = await _context.User.Include(x => x.Usertype).FirstOrDefaultAsync(y => y.Id == entity.Id);
            if ( userDetail != null )
            {
                var roleId = userDetail.UsertypeId;
                var newRole = userDetail.Usertype.Name;
                userDetail.UsertypeId = entity.UsertypeId;
                userDetail.FirstName = entity.FirstName;
                userDetail.LastName = entity.LastName;
                userDetail.Email = entity.Email;
                userDetail.Phone = entity.Phone ?? string.Empty;
                userDetail.PartnerId = entity.PartnerId;
                userDetail.ApiKey = entity.ApiKey;
                await _context.SaveChangesAsync();

                // update the role at fusionAuth if user exist
                if ( entity.UsertypeId != roleId && _appSettings.IsLoginUsingFusionAuth )
                {
                    var roleExist = await _context.UserType.FirstOrDefaultAsync(x => x.Id == entity.UsertypeId);
                    if( roleExist != null)
                    {
                        newRole = roleExist.Name;
                    }
                    var client = new io.fusionauth.FusionAuthSyncClient(EnvironmentVariables.FusionAuthApiKey , EnvironmentVariables.FusionAuthUrl, EnvironmentVariables.TenantId);
                    var userResponse = client.RetrieveUserByEmail(HttpUtility.UrlEncode(userDetail.Email));
                    if ( userResponse.WasSuccessful() && userResponse.statusCode == 200 )
                    {
                        var registrationResponse = UpdateRegistration(client , userResponse.successResponse.user , newRole);
                        if(!registrationResponse.WasSuccessful())
                        {
                            _logger.LogError($" Error from FusionAuth while update user role :{registrationResponse.exception.InnerException.Message}");
                        }
                    }
                }

                return userDetail;

            }
            return null;
        }

        public async Task ChangeUserStatusAsync (ChangeUserStatusModel entity)
        {
            var userStatus = await _context.UserStatus.FirstOrDefaultAsync(s => s.Name.Trim() == entity.UserStatus.ToString().Trim());
            if ( userStatus == null )
            {
                throw new BusinessException("The incorrect user status for the update.");
            }

            var user = await _context.User.SingleOrDefaultAsync(u => u.Id == entity.Id);
            if ( user == null )
            {
                throw new BusinessException("User not exist");
            }
            if ( user.PartnerId != null )
            {
                var isPartnerArchived = _context.Partner.Any(x => x.Id == user.PartnerId && x.IsArchived == true);
                if ( isPartnerArchived )
                {
                   throw new BusinessException("The partner is archived for this user.");
                }
            }

            user.UserstatusId = userStatus.Id;
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeUserPasswordAsync (string newPassword)
        {
            var userObj = new User
            {
                Password = newPassword
            };
            _context.Update(userObj);
            await _context.SaveChangesAsync();
        }

        //public async Task<WatchHistory> AddWatchHistory (WatchHistory entity)
        //{
        //    var watchHistory = await _context.WatchHistory.SingleOrDefaultAsync(x => x.UserId == entity.UserId && x.MediaId == entity.MediaId);
        //    if ( watchHistory != null )
        //    {
        //        watchHistory.LastWatchedUtc = entity.LastWatchedUtc;
        //        _context.Update(watchHistory);
        //    }
        //    else
        //    {
        //        await _context.AddAsync(entity);
        //    }
        //    await _context.SaveChangesAsync();
        //    return entity;
        //}

        //public async Task RemoveWatchHistoryAsync (long mediaId , int userId)
        //{
        //        var watchHistory = await _context.WatchHistory.SingleOrDefaultAsync(t => t.MediaId == mediaId && t.UserId == userId);
        //        if ( watchHistory != null )
        //        {
        //            _context.WatchHistory.Remove(watchHistory);
        //            await _context.SaveChangesAsync();
        //        }
        //}

        public async Task<string> GeneratResetPasswordToken (User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EnvironmentVariables.JwtSecret));
            var credentials = new SigningCredentials(securityKey , SecurityAlgorithms.HmacSha256Signature);

            var nonce = Guid.NewGuid().ToString("N").Substring(0 , 10);
            var securityToken = new JwtSecurityToken
            (
                EnvironmentVariables.JwtIssuer ,
                EnvironmentVariables.JwtAudience ,
                claims: new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Nonce, nonce)
                } ,
                expires: DateTime.Now.AddMinutes(1440) ,
                notBefore: DateTime.UtcNow ,
                signingCredentials: credentials
            );

            var resetToken = new JwtSecurityTokenHandler().WriteToken(securityToken);
            user.ResetPasswordNonce = nonce;
            _context.User.Update(user);
            await _context.SaveChangesAsync();
            return resetToken;
        }

        public async Task<bool> RemoveUserAsync (string email , bool onlyClean)
        {
            var dbUser = await _context.User.SingleOrDefaultAsync(t => t.Email.Trim() == email.Trim());
            if (dbUser == null)
                throw new BusinessException("User not found");

            var isDeleted = true;
            if ( dbUser != null )
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    // Remove User's favorites
                    var favorites = await _context.Favorites.Where(x => x.UserId == dbUser.Id).ToListAsync();
                    if (favorites.Count > 0)
                    {
                        _context.Favorites.RemoveRange(favorites);
                    }

                    // Remove User's Playlist with Media
                    var playlists = await _context.Playlist.Where(x => x.UserId == dbUser.Id).ToListAsync();
                    if (playlists.Count > 0)
                    {
                        var playlistIds = playlists.Select(x => x.Id).ToList();
                        if (playlistIds.Count > 0)
                        {
                            var playlistMedias = await _context.PlaylistMedia.Where(x => playlistIds.Contains(x.PlaylistId)).ToListAsync();
                            if (playlistMedias.Count > 0)
                            {
                                _context.PlaylistMedia.RemoveRange(playlistMedias);
                            }
                        }
                        // Remove Playlist attched to deleted User.
                        _context.Playlist.RemoveRange(playlists);
                    }

                    // Remove User's Watch History
                    var watchHistory = await _context.WatchHistory.Where(x => x.UserId == dbUser.Id).ToListAsync();
                    if (watchHistory.Count > 0)
                    {
                        _context.WatchHistory.RemoveRange(watchHistory);
                    }

                    // Remove User's Subscribed Series
                    var subsSeries = await _context.SubscriptionSeries.Where(x => x.UserId == dbUser.Id).ToListAsync();
                    if (subsSeries.Count > 0)
                    {
                        _context.SubscriptionSeries.RemoveRange(subsSeries);
                    }

                    // Remove User's Subscribed Topic
                    var subsTopic = await _context.SubscriptionTopic.Where(x => x.UserId == dbUser.Id).ToListAsync();
                    if (subsTopic.Count > 0)
                    {
                        _context.SubscriptionTopic.RemoveRange(subsTopic);
                    }

                    // Remove User's RefreshToken
                    var refreshToken = await _context.RefreshToken.Where(x => x.UserId == dbUser.Id).ToListAsync();
                    if (refreshToken.Count > 0)
                    {
                        _context.RefreshToken.RemoveRange(refreshToken);
                    }

                    if (!onlyClean)
                    {
                        // Remove User
                        _context.User.Remove(dbUser);
                    }

                    await _context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return isDeleted = false;
                }
            }

            return isDeleted;
        }

        public async Task<User> SubscribeNewsletter (int userId)
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

        private async Task SendSetPasswordMail (string receiverEmail , string firstName , string lastName , string invitingUserFName , string invitingUserLName , string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var userEmail = ( (JwtSecurityToken)jsonToken ).Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var userType = Users.Include(x => x.Usertype).Where(x => x.Email.Trim() == userEmail.Trim()).Select(x => x.Usertype.Name).SingleOrDefault();

            var callbackUrl = EnvironmentVariables.ClientUiUrl;
            if ( userType != "User" )
            {
                callbackUrl = EnvironmentVariables.AdminUiUrl;
            }
            callbackUrl += $"/setPassword?setPasswordToken={token}";
            var companyLogo = $"{EnvironmentVariables.ClientUiUrl}/ms_logo.png"; //await S3Utility.RetrieveImageWithSignedUrl("ms_logo.png"); // Need to replace company logo uuid when change

            var body = EmailTemplates.Templates[TestStories.Common.MailKit.Templates.UserInvite]
                .Replace("{{firstName}}" , firstName)
                .Replace("{{lastName}}" , lastName)
                .Replace("{{invitingUserFirstName}}" , invitingUserFName)
                .Replace("{{invitingUserLastName}}" , invitingUserLName)
                .Replace("{{productName}}" , "Million Stories")
                .Replace("{{companyLogo}}" , companyLogo)
                .Replace("{{resetLink}}" , callbackUrl);

            var email = new SendEmailBuilder(EnvironmentVariables.EmailQueue)
                .From(_emailSettings.From.noreply)
                .To(receiverEmail)
                .Subject(_emailSettings.Subject.UserInvitation)
                .Action(EmailActions.UserInvite)
                .Body(body)
                .Build();

            await _eventPublisher.Publish(email);
        }

        private async Task<User> AddUserAsync (User entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        static io.fusionauth.ClientResponse<io.fusionauth.domain.api.user.RegistrationResponse> UpdateRegistration (io.fusionauth.FusionAuthSyncClient client , FusionUser user , string role)
        {
            var registration = new io.fusionauth.domain.UserRegistration
            {
                roles = new List<string> { role } ,
                username = user.email ,
                applicationId = Guid.Parse(EnvironmentVariables.FusionAuthApplicationId)
            };
            var registrationRequest = new io.fusionauth.domain.api.user.RegistrationRequest
            {
                sendSetPasswordEmail = false ,
                skipRegistrationVerification = true ,
                skipVerification = true ,
                registration = registration
            };
            return client.UpdateRegistration(user.id , registrationRequest);
        }

        public async Task ImportUsersAtFusionAuth ()
        {
            _logger.LogInformation("Processing Db Users...");
            PagedResult<User> batch;
            var client = new io.fusionauth.FusionAuthSyncClient(EnvironmentVariables.FusionAuthApiKey , EnvironmentVariables.FusionAuthUrl, EnvironmentVariables.TenantId);
            
            var i = 1;

            do
            {
                batch = await _context.User.Include(y => y.Usertype).Where(x => x.UserstatusId == (int)UserStatusEnum.Active && !string.IsNullOrEmpty(x.Password)).AsQueryable().GetBatch(i , 100);
                i++;
                var users = new List<FusionUser>();
                foreach ( var entity in batch.Results )
                {
                    var fusionUser = client.RetrieveUserByEmail(HttpUtility.UrlEncode(entity.Email));
                    if(fusionUser.statusCode != 200)
                    {
                        var user = BuildUserImportRequest(entity);
                        users.Add(user);
                    }
                }
                if ( users.Count > 0 )
                {
                    var request = new io.fusionauth.domain.api.user.ImportRequest
                    {
                        users = users
                    };
                    var result = client.ImportUsers(request);

                    if(!result.WasSuccessful())
                    {
                        _logger.LogError($"Error while import users: {result.errorResponse}");
                    }
                }

            } while ( batch.Results.Count > 0 );

            _logger.LogInformation("Completed Users Transfer. OK");
        }
     
        static FusionUser BuildUserImportRequest (User user)
        {
            var userToCreate = new FusionUser
            {
                email = user.Email ,
                password = user.Password ,
                username = user.Email,
                fullName = user.Name,
                verified = true,
                active = true,
            };
            var data = new Dictionary<string , object>
            {
                { "original_user_id" , user.Id }
            };
            userToCreate.data = data;

            var registrations = new List<io.fusionauth.domain.UserRegistration>();
            var registration = new io.fusionauth.domain.UserRegistration
            {
                roles = new List<string> { user.Usertype.Name } ,
                username = user.Email, 
                applicationId = Guid.Parse(EnvironmentVariables.FusionAuthApplicationId),
                verified = true
            };
           
            registrations.Add(registration);
            userToCreate.registrations = registrations;
            return userToCreate;
        }

        public async Task<bool> RegisterActiveUser (RegisterActiveUser model)
        {
            if ( model.Email != null )
            {
                var isMailExist = _context.User.Any(x => x.Email == model.Email);
                if ( isMailExist )
                {
                    _logger.LogDebug($"Email already exist.");
                    throw new BusinessException("Email already exist");
                }
                if(_appSettings.IsLoginUsingFusionAuth)
                {
                    var client = new FusionAuthSyncClient(EnvironmentVariables.FusionAuthApiKey, EnvironmentVariables.FusionAuthUrl, EnvironmentVariables.TenantId);
                    var fusionUser = client.RetrieveUserByEmail(HttpUtility.UrlEncode(model.Email));
                    if (fusionUser.WasSuccessful() && fusionUser.statusCode == 200)
                    {
                        _logger.LogDebug($"User Already register with FusionAuth with Email: {model.Email}");
                        throw new BusinessException("User already registered with Singleton Foundation SSO, try with different email");
                    }
                }
            }

            var createDate = DateTime.UtcNow;

            var userEntity = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                UsertypeId = model.UserTypeId,
                UserstatusId = (byte)UserStatusEnum.Active,
                DateCreatedUtc = createDate,
            };
            var (adminUserTypeId, adminEditorUserTypeId, partnerUserTypeId) = await GetAdminPartnerTypeIdsAsync();
            if ( model.UserTypeId == adminUserTypeId || model.UserTypeId == adminEditorUserTypeId )
            {
                var parent = await _context.Partner.SingleOrDefaultAsync(x => x.Name == DefaultCompanyName);
                if ( parent != null )
                {
                    userEntity.PartnerId = parent.Id;
                }
            }
            if (_appSettings.IsLoginUsingFusionAuth)
            {
                return await RegisterUserTransactionUsingFusionAuth(userEntity, model);
            }
            else
            {
                try
                {
                    var responseFromDB = await AddUserAsync(userEntity);
                    return responseFromDB != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"something went wrong at db side: {ex.Message}");
                    return false;
                }
            }
           
        }

        private async Task<bool> RegisterUserTransactionUsingFusionAuth(User userEntity, RegisterActiveUser model)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var responseFromFusionAuth = false;
                try
                {
                    var responseFromDB = await AddUserAsync(userEntity);
                    if (responseFromDB != null)
                    {
                        responseFromDB.Usertype = await _context.UserType.FirstOrDefaultAsync(x => x.Id == responseFromDB.UsertypeId);
                        responseFromFusionAuth = RegisterUserAtFusionAuth(responseFromDB, model.Password);
                        if (responseFromFusionAuth)
                        {
                            transaction.Commit();
                            return responseFromFusionAuth;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"something went wrong at db side: {ex.Message}");
                    return false;
                }
                transaction.Rollback();
                return responseFromFusionAuth;

            }
            catch (Exception ex)
            {
                _logger.LogError($"something went wrong at either db side or FusionAuth side: {ex.Message}");
                transaction.Rollback();
                return false;
            }
        }

        public bool RegisterUserAtFusionAuth (User user , string plainPassword)
        {
            try
            {
                var client = new FusionAuthSyncClient(EnvironmentVariables.FusionAuthApiKey , EnvironmentVariables.FusionAuthUrl , EnvironmentVariables.TenantId);

                    var userRequest = BuildUserRequest(user , plainPassword);
                    var response = client.CreateUser(null , userRequest);

                    if ( response.WasSuccessful() && response.statusCode == 200 )
                    {
                        var fusionUser = response.successResponse.user;
                        var registrationResponse = Register(client , fusionUser , user.Usertype.Name);
                        if ( registrationResponse.WasSuccessful() && registrationResponse.statusCode == 200 )
                        {
                        return true;
                        }
                    }
                return false;
            }
            catch ( Exception ex )
            {
                _logger.LogError($"something went wrong at FusionAuth: {ex.Message}");
                return false;
            }
        }

        static UserRequest BuildUserRequest (User user , string plainPassword)
        {
            var userToCreate = new FusionUser
            {
                email = user.Email ,
                password = plainPassword ,
                username = user.Email ,
                fullName = user.Name

            };
            var data = new Dictionary<string , object>
            {
                { "original_user_id" , user.Id }
            };
            userToCreate.data = data;

            var userRequest = new UserRequest
            {
                sendSetPasswordEmail = false ,
                user = userToCreate
            };
            return userRequest;
        }

        static ClientResponse<RegistrationResponse> Register (FusionAuthSyncClient client , FusionUser user , string role)
        {
            var registration = new UserRegistration
            {
                roles = new List<string> { role } ,
                username = user.email ,
                applicationId = Guid.Parse(EnvironmentVariables.FusionAuthApplicationId)
            };

            var registrationRequest = new RegistrationRequest
            {
                sendSetPasswordEmail = false ,
                skipRegistrationVerification = true ,
                skipVerification = true ,
                registration = registration
            };
            return client.Register(user.id , registrationRequest);
        }

        public async Task<UserResponseModel> GetUserByEmail(string email)
        {
            var result = new UserResponseModel();
            var dbUser = await _context.User.FirstOrDefaultAsync(z => z.Email == email);
            result.DbUserDetails = dbUser;

            if(_appSettings.IsLoginUsingFusionAuth)
            {
                var client = new io.fusionauth.FusionAuthSyncClient(EnvironmentVariables.FusionAuthApiKey, EnvironmentVariables.FusionAuthUrl, EnvironmentVariables.TenantId);
                var userResponse = client.RetrieveUserByEmail(HttpUtility.UrlEncode(email));
                if (userResponse.WasSuccessful() && userResponse.statusCode == 200)
                {
                    result.FusionUser = userResponse.successResponse?.user;
                }
            }

            return result;
        }

        public async Task UpdateApiKeyByEmail(string email, string apiKey, ClaimsPrincipal claimsPrincipal)
        {
            var roles = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? UserTypeEnum.User.ToString();
            if (!roles.Contains("SuperAdmin"))
            {
               throw new BusinessException("Invalid User");
            }

            var userDetail = await _context.User.SingleOrDefaultAsync(x => x.Email == email);

            userDetail.ApiKey = apiKey;
            
            await _context.SaveChangesAsync();
        }
    }
}
