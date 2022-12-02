using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;

namespace TestStories.API.Health
{
    internal class S3Check : IHealthCheck
    {
        private readonly S3Options _options;

        public S3Check(S3Options options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (options.BucketName == null)
            {
                throw new ArgumentNullException(nameof(S3Options.BucketName));
            }
            if (options.Endpoint == null)
            {
                throw new ArgumentNullException(nameof(S3Options.Endpoint));
            }
            _options = options;
        }
        
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = new AmazonS3Client(_options.Endpoint))
                {
                    var response = await client.ListObjectsAsync(_options.BucketName, cancellationToken);

                    if (_options?.CustomResponseCheck != null)
                    {
                        return _options.CustomResponseCheck.Invoke(response)
                            ? HealthCheckResult.Healthy()
                            : new HealthCheckResult(context.Registration.FailureStatus, description: "Response check is not satisfied.");
                    }
                }
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}