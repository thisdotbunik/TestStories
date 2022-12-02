using System.Collections.Generic;
using TestStories.API.Models.ResponseModels;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SaveEditorPickModel
    {
        [JsonProperty(propertyName: "collection")]
        public ICollection<EditorPickModel> Collection { get; set; }
    }
}
