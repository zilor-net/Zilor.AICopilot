using Microsoft.AspNetCore.Identity;

namespace Zilor.AICopilot.Services.Contracts;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(IdentityUser user);
}