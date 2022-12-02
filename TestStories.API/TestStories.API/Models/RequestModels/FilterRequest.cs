using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TestStories.DataAccess.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TestStories.API.Models.RequestModels
{
    /// <summary>
    /// Filter Request
    /// </summary>
    public class FilterRequest
    {
        private const int DefaultPageSize = 10;

        [EnumDataType(typeof(SortOrderEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(propertyName: "sortOrder")]
        [DefaultValue(SortOrderEnum.Ascending)]
        public SortOrderEnum SortOrder { get; set; } = SortOrderEnum.Ascending;

        [JsonProperty(propertyName: "filterString")]
        [DefaultValue("")]
        public string FilterString { get; set; } = "";

        [JsonProperty(propertyName: "page")]
        [DefaultValue(1)]
        public int Page { get; set; } = 1;

        [JsonProperty(propertyName: "pageSize")]
        [DefaultValue(DefaultPageSize)]
        public int PageSize { get; set; } = DefaultPageSize;
    }
}
