using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class NewletterResponse
    {
        [JsonProperty("userId")] public int UserId { get; set; }

        [JsonProperty("userName")] public string UserName { get; set; }

        [JsonProperty("firstName")] public string FirstName { get; set; }

        [JsonProperty("lastName")] public string LastName { get; set; }
        [JsonProperty("isNewsletterSubsribed")] public bool IsNewsletterSubsribed { get; set; }
    }
}
