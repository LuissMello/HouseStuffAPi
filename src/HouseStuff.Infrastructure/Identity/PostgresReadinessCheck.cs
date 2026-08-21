using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HouseStuff.Infrastructure.Identity;

public sealed class PostgresReadinessCheck(HouseStuffDbContext database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await database.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("PostgreSQL disponível.")
                : HealthCheckResult.Unhealthy("PostgreSQL indisponível.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL indisponível.", exception);
        }
    }
}
