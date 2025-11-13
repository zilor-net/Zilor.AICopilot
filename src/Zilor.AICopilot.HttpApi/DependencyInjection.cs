using Microsoft.AspNetCore.Identity;
using Zilor.AICopilot.EntityFrameworkCore;

namespace Zilor.AICopilot.HttpApi;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructures(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureIdentity(services);
    
        return services;
    }

    private static void ConfigureIdentity(IServiceCollection services)
    {
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AiCopilotDbContext>();
    }
}