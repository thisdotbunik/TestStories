using System;
using Amazon.DynamoDBv2.DataModel;

namespace TestStories.API.Models
{
    [DynamoDBTable("blogs", LowerCamelCaseProperties=true)]
    public class Blogs
    {

        [DynamoDBHashKey]
        public string Id { get; set; }


        [DynamoDBProperty]
        public string BlogId { get; set; }

        [DynamoDBProperty]
        public string Title { get; set; }

        [DynamoDBProperty]
        public string Description { get; set; }

        [DynamoDBProperty]
        public string FeaturedImage { get; set; }

        [DynamoDBProperty]
        public string Url { get; set; }

        [DynamoDBProperty]
        public DateTime FetchedAt { get; set; }

        [DynamoDBProperty]
        public DateTime PublishedDate { get; set; }
    }
}
