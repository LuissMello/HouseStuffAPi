using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HouseStuff.Infrastructure.Identity;

public sealed class PostgresReadinessCheck(HouseStuffDbContext database, StartupState startup) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!startup.IsReady)
        {
            return HealthCheckResult.Unhealthy("Inicialização do banco em andamento.");
        }

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
