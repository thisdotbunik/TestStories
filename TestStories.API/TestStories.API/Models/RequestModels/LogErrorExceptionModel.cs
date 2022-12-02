using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class LogErrorExceptionModel
    {

        [DefaultValue("")]
        [JsonProperty(propertyName: "exception")]
        public string Exception { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "stackTrace")]
        public string StackTrace { get; set; }
    }
}
