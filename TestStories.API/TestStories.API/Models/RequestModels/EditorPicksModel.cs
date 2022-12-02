using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class EditorPicksModel
    {
        [DefaultValue("")]
        [JsonProperty("title")]
        [Required]
        public string Title { get; set; }

        [DefaultValue("")]
        [JsonProperty("embeddedCode")]
        [Required]
        public string EmbeddedCode { get; set; }
    }
}
