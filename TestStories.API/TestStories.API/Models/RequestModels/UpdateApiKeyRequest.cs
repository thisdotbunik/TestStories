using TestStories.API.Validators;

namespace TestStories.API.Models.RequestModels
{
    public class UpdateApiKeyRequest
    {
        [ApiKey(16)]
        public string ApiKey { get; set; }
    }
}
