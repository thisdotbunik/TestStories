namespace TestStories.API.Models.ResponseModels
{
    public class SeoModel
    {
        public long EntityId { get; set; }
        public byte EntityTypeId { get; set; }
        public string PrimaryKeyword { get; set; }
        public string SecondaryKeyword { get; set; }
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
        public string PageDescription { get; set; }
        public string H1 { get; set; }
        public string H2 { get; set; }
    }
}
