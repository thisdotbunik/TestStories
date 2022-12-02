using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Services.Validators
{
    public class EditSeriesModelValidator : AbstractValidator<EditSeriesModel>
    {
        public EditSeriesModelValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
            RuleFor(x => x.SeriesTitle).NotNull().NotEmpty().MaximumLength(40);
            RuleFor(x => x.SeriesDescription).MaximumLength(1000);
            RuleFor(x => x.SeriesDescriptionColor).MaximumLength(8);
        }
    }
}
