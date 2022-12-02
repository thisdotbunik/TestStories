namespace TestStories.API.Models.ResponseModels
{
    public class ExportResourceModel
    {
        public int ResourceId { get; set; }
        public string ResourceTitle { get; set; }  
        public string Description { get; set; }
        public string Url { get; set; }
        public string Topics { get; set; }
        public string AssignedTo { get; set; }
        public string ResourceType { get; set; }
        public string SelectedPartner { get; set; }
        public string FeaturedImage { get; set; }
        public string ShowOnMenu { get; set; }
        public string ShowOnHomePage { get; set; }
        public string DateCreated { get; set; }
    }
}
