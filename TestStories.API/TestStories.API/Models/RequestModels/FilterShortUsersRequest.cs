using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TestStories.DataAccess.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TestStories.API.Models.RequestModels
{
    /// <inheritdoc />
    public class FilterShortUsersRequest : FilterRequest
    {
        [EnumDataType(typeof(ShortUsersSortingEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(propertyName: "sortedProperty")]
        [DefaultValue(ShortUsersSortingEnum.DateAdded)]
        public ShortUsersSortingEnum SortedProperty { get; set; } = ShortUsersSortingEnum.DateAdded;
    }
}
