using System.Collections.Generic;

namespace TestStories.API.Models.ResponseModels
{
    public class GridResponse<T>: BaseResponse
    {
        public List<T> items { get; set; }
        public int TotalRowsAvailable { get; set; }
    }
}
