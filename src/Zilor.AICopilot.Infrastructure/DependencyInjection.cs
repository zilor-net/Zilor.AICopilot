using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zilor.AICopilot.EntityFrameworkCore;
using Zilor.AICopilot.IdentityService.Contracts;
using Zilor.AICopilot.Infrastructure.Authentication;

namespace Zilor.AICopilot.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructures(this IHostApplicationBuilder builder)
    {
        builder.AddEfCore();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    }
}