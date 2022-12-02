using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Services.Validators
{
    public class EditTopicModelValidator : AbstractValidator<EditTopicModel>
    {
        public EditTopicModelValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
            RuleFor(x => x.TopicName).NotNull().NotEmpty().MaximumLength(40);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }
}
