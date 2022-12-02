using System;
using Amazon;
using Amazon.S3.Model;

namespace TestStories.API.Health
{
    public class S3Options
    {
        public RegionEndpoint Endpoint  { get; set; }

        public string BucketName { get; set; }

        public Func<ListObjectsResponse, bool> CustomResponseCheck { get; set; }
    }
}