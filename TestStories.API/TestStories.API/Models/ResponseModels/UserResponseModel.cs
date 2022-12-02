using TestStories.DataAccess.Entities;

namespace TestStories.API.Models.ResponseModels
{
    public class UserResponseModel
    {
        public User DbUserDetails { get; set; }
        public io.fusionauth.domain.User FusionUser { get; set; }
    }
}
