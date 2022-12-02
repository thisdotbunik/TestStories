using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace TestStories.API.Health
{
    internal class LivenessHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult>
            CheckHealthAsync(HealthCheckContext context,
                CancellationToken cancellationToken = default) =>
            Task.FromResult(HealthCheckResult.Healthy());
    }
}