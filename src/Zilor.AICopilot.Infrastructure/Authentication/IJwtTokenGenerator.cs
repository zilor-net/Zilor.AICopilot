using Microsoft.AspNetCore.Identity;

namespace Zilor.AICopilot.Infrastructure.Authentication;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(IdentityUser user);
}