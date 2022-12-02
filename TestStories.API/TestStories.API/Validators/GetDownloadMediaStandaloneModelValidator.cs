using FluentValidation;
using FluentValidation.AspNetCore;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Validators
{
    public class GetDownloadMediaStandaloneModelValidator : AbstractValidator<GetDownloadMediaStandaloneModel>
    {
        public GetDownloadMediaStandaloneModelValidator()
        {
            RuleFor(x => x.ApiKey).NotEmpty().NotNull();
            RuleFor(x => x.Id).NotEmpty().NotNull().GreaterThan(0);
        }

    }
}
