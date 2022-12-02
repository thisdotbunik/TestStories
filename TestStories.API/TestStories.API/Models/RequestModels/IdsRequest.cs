using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class IdsRequest<T>
    {
        [JsonProperty(propertyName: "ids")]
        public ICollection<T> Ids { get; set; }
    }
}
