using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Zilor.AICopilot.IdentityService;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(IdentityUser user);
}