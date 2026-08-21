using HouseStuff.Api.ProjectTracking;
using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();

public partial class Program;
