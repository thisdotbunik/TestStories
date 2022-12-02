using Newtonsoft.Json;

namespace TestStories.API.Services.Errors
{
    public class ApiError
    {
        [JsonProperty("status")]
        public int Status { get; private set; }

        [JsonProperty("title")]
        public string Title { get; private set; }

        [JsonProperty("detail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Detail { get; private set; }

        /// <inheritdoc />
        public ApiError(int status, string title)
        {
            Status = status;
            Title = title;
        }

        /// <inheritdoc />
        public ApiError(int status, string title, string detail)
            : this(status, title)
        {
            Detail = detail;
        }
    }
}
