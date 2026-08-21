using HouseStuff.Api.Maintenance;
using HouseStuff.Api.ProjectTracking;
using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// Argumentos de manutenção são posicionais e o provider de linha de comando só aceita `--chave=valor`.
var maintenance = MaintenanceCommands.IsRequested(args);
var builder = WebApplication.CreateBuilder(maintenance ? [] : args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IProjectTrackingReader, ProjectTrackingReader>();
builder.Services.AddHealthChecks().AddCheck<PostgresReadinessCheck>("postgres", tags: ["ready"]);
builder.Services.AddHouseStuffIdentity(builder.Configuration, builder.Environment);
builder.Services.AddCors(options => options.AddPolicy("frontend", policy => policy
    .WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapControllers();

await app.Services.InitializeHouseStuffIdentityAsync();

if (maintenance)
{
    return await MaintenanceCommands.RunAsync(app.Services, args, CancellationToken.None);
}

app.Run();

return 0;

public partial class Program;
