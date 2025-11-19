using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zilor.AICopilot.EntityFrameworkCore.Repository;
using Zilor.AICopilot.SharedKernel.Repository;

namespace Zilor.AICopilot.EntityFrameworkCore;

public static class DependencyInjection
{
    public static void AddEfCore(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<AiCopilotDbContext>("ai-copilot");
        
        builder.Services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        
        builder.Services.AddIdentityCore<IdentityUser>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AiCopilotDbContext>();
    }
}