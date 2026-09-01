using Microsoft.OpenApi.Models;

namespace HouseStuff.Api.OpenApi;

public static class ApiDocumentationExtensions
{
    public static IServiceCollection AddHouseStuffApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HouseStuff API",
                Version = "v1",
                Description = "API da casa para usuários, potes, tarefas, calendário e compras."
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "Token opaco",
                Description = "Informe somente o accessToken retornado por POST /api/v1/auth/login."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }] = Array.Empty<string>()
            });
        });
        return services;
    }

    public static WebApplication UseHouseStuffApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "HouseStuff API";
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "HouseStuff API v1");
            options.DisplayRequestDuration();
        });
        return app;
    }
}
