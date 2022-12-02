using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MillionStories.API.Entities
{
    public class GridResponse<T>: BaseResponse
    {
        public List<T> Records { get; set; }
        public int TotalRowsAvailable { get; set; }
    }
}
