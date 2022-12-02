using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MillionStories.API.Entities
{
    public class BaseResponse
    {
        public int ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}
