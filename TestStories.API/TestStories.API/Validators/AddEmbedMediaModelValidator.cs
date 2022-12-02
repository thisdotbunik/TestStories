using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Services.Validators
{
    /// <inheritdoc />
    public class AddEmbedMediaModelValidator : AbstractValidator<AddEmbedMediaModel>
    {
        /// <inheritdoc />
        public AddEmbedMediaModelValidator()
        {
            RuleFor(x => x.Title).NotNull().NotEmpty().MaximumLength(90);
            RuleFor(x => x.EmbedCode).NotNull().NotEmpty().MaximumLength(500);
        }
    }
}
