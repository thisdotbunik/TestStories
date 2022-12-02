namespace TestStories.API.Models.RequestModels
{
    public class FilterMediaSearchRequest
    {
        public int[] mediaStatusIds { get; set; }
        public int[] mediaTypeIds { get; set; }
    }
}
