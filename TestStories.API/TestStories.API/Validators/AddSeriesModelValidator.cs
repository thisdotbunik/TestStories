using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Services.Validators
{
    public class AddSeriesModelValidator : AbstractValidator<AddSeriesModel>
    {
        public AddSeriesModelValidator()
        {
            RuleFor(x => x.SeriesTitle).NotNull().NotEmpty().MaximumLength(40);
            RuleFor(x => x.SeriesDescription).MaximumLength(1000);
            RuleFor(x => x.SeriesDescriptionColor).MaximumLength(8);
        }
    }
}
