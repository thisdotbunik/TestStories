using System;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ShortUserModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(propertyName: "lastName")]
        public string LastName { get; set; }

        [JsonProperty(propertyName: "email")]
        public string Email { get; set; }

        [JsonProperty(propertyName: "userTypeId")]
        public int UserTypeId { get; set; }

        [JsonProperty(propertyName: "partnerId")]
        public int? PartnerId { get; set; }

        [JsonProperty(propertyName: "userType")]
        public string UserType { get; set; }

        [JsonProperty(propertyName: "company")]
        public string Company { get; set; }

        [JsonProperty(propertyName: "dateAdded")]
        public DateTime DateAdded { get; set; }

        [JsonProperty(propertyName: "status")]
        public string Status { get; set; }

        [JsonProperty(propertyName: "phone")]
        public string Phone { get; set; }

        [JsonProperty("isNewsletterSubscribed")] 
        public bool IsNewsletterSubscribed { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }
    }
}
