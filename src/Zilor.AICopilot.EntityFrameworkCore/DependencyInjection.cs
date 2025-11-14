using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Zilor.AICopilot.EntityFrameworkCore;

public static class DependencyInjection
{
    public static void AddIdentityEfCore(this IServiceCollection services)
    {
        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AiCopilotDbContext>();
    }
}