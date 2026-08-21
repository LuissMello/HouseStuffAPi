using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace HouseStuff.Infrastructure.UnitTests;

public sealed class IdentityCookieOptionsTests
{
    [Fact]
    public void ProductionCookieAllowsSecureCrossSiteFrontend()
    {
        var options = BuildOptions("Production");

        Assert.Equal(SameSiteMode.None, options.Cookie.SameSite);
        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.True(options.Cookie.HttpOnly);
    }

    [Fact]
    public void DevelopmentCookieRemainsLocalAndStrict()
    {
        var options = BuildOptions("Development");

        Assert.Equal(SameSiteMode.Strict, options.Cookie.SameSite);
        Assert.Equal(CookieSecurePolicy.SameAsRequest, options.Cookie.SecurePolicy);
    }

    private static CookieAuthenticationOptions BuildOptions(string environmentName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HouseStuff"] = "Host=localhost;Database=housestuff;Username=test;Password=test",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHouseStuffIdentity(configuration, new TestWebHostEnvironment(environmentName));

        using var provider = services.BuildServiceProvider();
        return provider
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);
    }

    private sealed class TestWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "HouseStuff.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = environmentName;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
