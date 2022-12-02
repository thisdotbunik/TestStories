using System.Net;
using TestStories.API.Services.Errors;

namespace TestStories.API.Infrastructure.Errors
{
    /// <inheritdoc />
    public class BusinessValidationError : ApiError
    {
        /// <inheritdoc />
        public BusinessValidationError() 
            : base(422, HttpStatusCode.UnprocessableEntity.ToString())
        {
        }

        /// <inheritdoc />
        public BusinessValidationError(string detail) 
            : base(422, HttpStatusCode.UnprocessableEntity.ToString(), detail)
        {
        }
    }
}
