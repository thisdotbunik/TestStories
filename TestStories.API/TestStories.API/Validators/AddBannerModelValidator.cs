using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Services.Validators
{
    /// <inheritdoc />
    public class AddBannerModelValidator : AbstractValidator<AddBannerModel>
    {
        public AddBannerModelValidator()
        {
            RuleFor(x => x.Title).NotNull().NotEmpty().MaximumLength(90);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }
}
