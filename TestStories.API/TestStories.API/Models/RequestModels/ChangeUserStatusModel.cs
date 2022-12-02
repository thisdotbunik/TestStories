using TestStories.DataAccess.Enums;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class ChangeUserStatusModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "userStatus")]
        public UserStatusEnum UserStatus { get; set; }
    }
}
