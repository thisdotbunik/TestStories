using System.Text.RegularExpressions;
using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Services.Validators
{
    /// <inheritdoc />
    public class AddUserModelValidator : AbstractValidator<AddUserModel>
    {
        /// <inheritdoc />
        public AddUserModelValidator()
        {
            RuleFor(x => (int) x.UserTypeId).NotNull().InclusiveBetween(1, 4);
            RuleFor(x => x.FirstName).NotNull().NotEmpty().MaximumLength(25);
            RuleFor(x => x.LastName).NotNull().NotEmpty().MaximumLength(40);
            RuleFor(x => x.Email).NotNull().NotEmpty().MaximumLength(100).EmailAddress();
           // RuleFor(x => x.Phone).Null().Empty().MaximumLength(12).Matches(new Regex(@"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}"));
            //RuleFor(x => x.PartnerId).Null();
        }
    }
}
