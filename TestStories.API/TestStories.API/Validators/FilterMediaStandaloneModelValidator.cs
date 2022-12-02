using System;
using System.Linq;
using FluentValidation;
using TestStories.API.Models.RequestModels;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Validators
{
    public class FilterMediaStandaloneModelValidator : AbstractValidator<FilterMediaStandaloneModel>
    {
        public FilterMediaStandaloneModelValidator()
        {
            RuleFor(x => x.ApiKey).NotEmpty().NotNull();
            RuleFor(x => x.Fields).NotEmpty().NotNull();
            RuleFor(x => x.Ids).NotEmpty().NotNull();
            RuleFor(x => x.MediaTypes).NotEmpty().NotNull();

            RuleFor(x => x.Ids)
            .Must(value => value.Split(",").All(id => int.TryParse(id, out int num)))
            .WithMessage("Media Ids should be value of \"all\" or comma separated list of ids")
            .When(x => !String.IsNullOrEmpty(x.Ids) && !x.Ids.Equals("all", StringComparison.OrdinalIgnoreCase));

            RuleFor(x => x.MediaTypes)
            .Must(value => value.Split(",").All(mediaType => MediaTypeEnum.TryParse(mediaType, out MediaTypeEnum typeEnum)))
            .WithMessage("Media Types should be value of \"all\" or comma separated list of Video, PodcastAudio, EmbeddedMedia, Banner")
            .When(x => !String.IsNullOrEmpty(x.MediaTypes) && !x.MediaTypes.Equals("all", StringComparison.OrdinalIgnoreCase));
        }
    }
}
