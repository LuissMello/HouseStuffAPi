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

if (maintenance)
{
    await app.Services.InitializeHouseStuffIdentityAsync();
    return await MaintenanceCommands.RunAsync(app.Services, args, CancellationToken.None);
}

// A porta precisa abrir antes das migrations: o proxy do Fly desiste da máquina se demorarmos
// para ficar acessíveis, e o banco fica em outra região. O readiness segura o tráfego até o fim.
await app.StartAsync();
await app.Services.InitializeHouseStuffIdentityAsync();
app.Services.GetRequiredService<StartupState>().MarkReady();
await app.WaitForShutdownAsync();

return 0;

public partial class Program;
