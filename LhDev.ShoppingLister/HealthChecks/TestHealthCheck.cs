using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Idox.Prototype.DmsAgent.HealthChecks;

public class TestHealthCheck :IHealthCheck
{
    private Random _random = new Random();

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var responseTime = _random.Next(1, 300);
        return responseTime switch
        {
            < 100 => Task.FromResult(HealthCheckResult.Healthy($"Healthy result ({responseTime})")),
            < 200 => Task.FromResult(HealthCheckResult.Degraded($"Degraded result ({responseTime})")),
            _ => Task.FromResult(HealthCheckResult.Unhealthy($"Unhealthy result ({responseTime})"))
        };
    }
}