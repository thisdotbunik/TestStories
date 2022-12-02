using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class FilterUserRequest
    {
        [DefaultValue("")]
        [JsonProperty(propertyName: "filterString")]
        public string FilterString { get;set;}

        [DefaultValue("")]
        [JsonProperty(propertyName: "sortedProperty")]
        public string SortedProperty { get;set; }

        [DefaultValue("ascending")]
        [JsonProperty(propertyName: "sortOrder")]
        public string SortOrder { get;set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "userType")]
        public string UserType { get;set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "userStatus")]
        public string UserStatus { get;set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "fromDate")]
        public DateTime? FromDate { get;set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "toDate")]
        public DateTime? ToDate { get;set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "company")]
        public string Company { get; set; }

        [DefaultValue(1)]
        [JsonProperty(propertyName: "page")]
        public int Page { get; set; }

        [DefaultValue(10)]
        [JsonProperty(propertyName: "pageSize")]
        public int PageSize { get; set; }
    }
}
