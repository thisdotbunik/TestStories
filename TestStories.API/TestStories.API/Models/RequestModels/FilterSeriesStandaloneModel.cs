using System.ComponentModel;
using DocumentFormat.OpenXml.Presentation;

namespace TestStories.API.Models.RequestModels
{
    public class FilterSeriesStandaloneModel
    {
        public string ApiKey { get; set; }

        [DefaultValue("all")]
        public string Fields { get; set; }

        [DefaultValue("all")]
        public string SeriesId { get; set; }
    }
}
