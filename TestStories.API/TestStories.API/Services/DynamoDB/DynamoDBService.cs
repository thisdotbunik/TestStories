using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using TestStories.API.Models;
using TestStories.Common;

namespace TestStories.API.Services
{
    /// <summary>
    /// Wrapper class that consumed by Video Pipeline
    /// </summary>
    public static class DynamoDBService
    {
        private static readonly RegionEndpoint Endpoint = RegionEndpoint.GetBySystemName(EnvironmentVariables.AwsRegion);
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger("VideoLibrary");
        private static readonly DynamoDBContext Ctx;

        /// <inheritdoc />
        static DynamoDBService()
        {
            var client = new AmazonDynamoDBClient(Endpoint);
            var config = new DynamoDBContextConfig()
            {
                TableNamePrefix = $"{EnvironmentVariables.Prefix}-{EnvironmentVariables.Env}-"
            };
            Ctx = new DynamoDBContext(client, config);
        }

        /// <summary>
        /// Update Video Record
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task Save(VideoMetadata metadata)
        {
            try
            {
                await Ctx.SaveAsync(metadata);
            }
            catch (AmazonServiceException e)
            {
                Logger.LogError("Error encountered on server. Message:'{0}' when writing an object to DynamoDB", e.Message);
                throw new ArgumentException("DynamoDB write error");
            }
            catch (Exception e)
            {
                Logger.LogError("Unknown encountered on server. Message:'{0}' when writing an object to DynamoDB", e.Message);
                throw new ArgumentException("DynamoDB write error");
            }
        }

        /// <summary>
        /// Find Video Record by uuid
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Task<VideoMetadata> FindById(string uuid)
        {
            return Ctx.LoadAsync<VideoMetadata>(uuid);
        }


        /// <summary>
        /// Find Blogs from DynamoDB
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<List<Blogs>> GetBlogs ()
        {
            var client = new AmazonDynamoDBClient(Endpoint);
            var conditions = new List<ScanCondition>();
            var db = new DynamoDBContext(client, new DynamoDBContextConfig() { TableNamePrefix = $"{EnvironmentVariables.Prefix}-{EnvironmentVariables.Env}-" });
            return await db.ScanAsync<Blogs>(conditions).GetRemainingAsync();
        }
    }
}