using HouseStuff.Application.Identity;
using HouseStuff.Application.Residences;
using HouseStuff.Infrastructure.Residences;
using HouseStuff.Application.Pots;
using HouseStuff.Infrastructure.Pots;
using HouseStuff.Application.Tasks;
using HouseStuff.Infrastructure.Tasks;
using HouseStuff.Application.Assignments;
using HouseStuff.Infrastructure.Assignments;
using HouseStuff.Application.Routine;
using HouseStuff.Infrastructure.Routine;
using HouseStuff.Application.Shopping;
using HouseStuff.Infrastructure.Shopping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HouseStuff.Infrastructure.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddHouseStuffIdentity(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("HouseStuff")
            ?? throw new InvalidOperationException("ConnectionStrings:HouseStuff não foi configurada.");

        services.AddDbContext<HouseStuffDbContext>(options => options.UseNpgsql(connectionString));
        services.AddIdentity<HouseStuffUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 10;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<HouseStuffDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "HouseStuff.Session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
                : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserAccessService, UserAccessService>();
        services.AddScoped<IResidenceService, ResidenceService>();
        services.AddScoped<ICurrentResidenceContext, CurrentResidenceContext>();
        services.AddScoped<IPotService, PotService>();
        services.AddScoped<IHouseholdTaskService, HouseholdTaskService>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();
        services.AddScoped<IRoutineOverviewService, RoutineOverviewService>();
        services.AddScoped<IShoppingCatalogService, ShoppingCatalogService>();
        return services;
    }

    public static async Task InitializeHouseStuffIdentityAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<HouseStuffDbContext>();
        await database.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { HouseStuffRoles.Administrator, HouseStuffRoles.Member })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        if (!environment.IsDevelopment())
        {
            return;
        }

        var email = configuration["DevelopmentAdmin:Email"];
        var password = configuration["DevelopmentAdmin:Password"];
        var name = configuration["DevelopmentAdmin:Name"] ?? "Administrador";
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<HouseStuffUser>>();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new HouseStuffUser { UserName = email, Email = email, Name = name };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Não foi possível criar o administrador local.");
            }
        }

        if (!await userManager.IsInRoleAsync(user, HouseStuffRoles.Administrator))
        {
            await userManager.AddToRoleAsync(user, HouseStuffRoles.Administrator);
        }

        await DevelopmentDemoSeeder.SeedAsync(database, userManager, user, configuration);
    }
}
