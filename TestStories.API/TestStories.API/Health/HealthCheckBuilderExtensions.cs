using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TestStories.API.Health
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddLivenessHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus,
            IEnumerable<string> tags)
        {
            return builder.AddCheck<LivenessHealthCheck>(
                name,
                failureStatus,
                tags);
        }

        public static IHealthChecksBuilder AddS3Check(
            this IHealthChecksBuilder builder,
            Action<S3Options> setup,
            string name,
            HealthStatus? failureStatus,
            IEnumerable<string> tags)
        {
            var options = new S3Options();
            setup?.Invoke(options);

            return builder.Add(registration: new HealthCheckRegistration(
                name: name ?? "s3",
                sp => new S3Check(options),
                failureStatus: failureStatus,
                tags: tags));
        }
    }
}