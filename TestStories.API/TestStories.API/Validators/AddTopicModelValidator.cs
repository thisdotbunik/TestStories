using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Services.Validators
{
    /// <inheritdoc />
    public class AddTopicModelValidator : AbstractValidator<AddTopicModel>
    {
        /// <inheritdoc />
        public AddTopicModelValidator()
        {
            RuleFor(x => x.TopicName).NotNull().NotEmpty().MaximumLength(40);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }
}
