using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class PartnerDistributionDetailsViewModel
    {
        public List<PartnerDistributionViewModel> Items { get; set; }
        public int Count { get; set; }
    }
    public class PartnerDistributionViewModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "partnerId")]
        public int PartnerId { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "partner")]
        public string Partner { get; set; }

        [JsonProperty(propertyName: "shareWith")]
        public string ShareWith { get; set; }

        [JsonProperty(propertyName: "startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(propertyName: "endDate")]
        public DateTime EndDate { get; set; }

    }
}
