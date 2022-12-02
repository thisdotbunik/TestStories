using System;
using System.Linq;
using FluentValidation;
using TestStories.API.Models.RequestModels;

namespace TestStories.API.Validators
{
    public class FilterSeriesStandaloneModelValidator : AbstractValidator<FilterSeriesStandaloneModel>
    {
        public FilterSeriesStandaloneModelValidator()
        {
            RuleFor(x => x.ApiKey).NotEmpty().NotNull();
            RuleFor(x => x.Fields).NotEmpty().NotNull();
            RuleFor(x => x.SeriesId).NotEmpty().NotNull();

            RuleFor(x => x.SeriesId)
    .Must(value => value.Split(",").All(id => int.TryParse(id, out int num)))
    .WithMessage("SeriesId should be value of \"all\" or comma separated list of ids")
    .When(x => !String.IsNullOrEmpty(x.SeriesId) && !x.SeriesId.Equals("all", StringComparison.OrdinalIgnoreCase));
        }
    }
}
