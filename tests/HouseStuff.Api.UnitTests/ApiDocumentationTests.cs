using HouseStuff.Api.Controllers;
using HouseStuff.Api.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace HouseStuff.Api.UnitTests;

public sealed class ApiDocumentationTests
{
    [Fact]
    public void RegistersPublicV1DocumentWithBearerAuthentication()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
        builder.Services.AddHouseStuffApiDocumentation();

        using var app = builder.Build();
        var document = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

        Assert.Equal("HouseStuff API", document.Info.Title);
        Assert.Contains("/api/v1/auth/login", document.Paths.Keys);
        var bearer = Assert.Contains("Bearer", document.Components.SecuritySchemes);
        Assert.Equal(SecuritySchemeType.Http, bearer.Type);
        Assert.Equal("bearer", bearer.Scheme);
    }
}
