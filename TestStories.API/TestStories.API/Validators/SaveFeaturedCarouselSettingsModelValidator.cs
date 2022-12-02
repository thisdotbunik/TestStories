using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Services.Validators
{
    /// <inheritdoc />
    public class SaveFeaturedCarouselSettingsModelValidator : AbstractValidator<SaveFeaturedCarouselSettingsModel>
    {
        /// <inheritdoc />
        public SaveFeaturedCarouselSettingsModelValidator()
        {
            RuleFor(x => x.Randomize).NotNull();
            RuleFor(x => x.SetByAdmin).NotNull();
            RuleFor(x => x.Ids).ForEach(item => item.NotNull().GreaterThan(0));

            When(x => !x.Randomize, () => { RuleFor(item => item.SetByAdmin).NotEmpty(); });
            When(x => !x.SetByAdmin, () => { RuleFor(item => item.Randomize).NotEmpty(); });
        }
    }
}
