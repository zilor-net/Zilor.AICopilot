using System.Security.Claims;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.HttpApi.Infrastructure;

public class CurrentUser : ICurrentUser
{
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user == null) return;

        if (!user.Identity!.IsAuthenticated) return;

        Id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        UserName = user.FindFirstValue(ClaimTypes.Name);
        Role = user.FindFirstValue(ClaimTypes.Role);

        IsAuthenticated = true;
    }

    public string? Id { get; }
    public string? UserName { get; }
    public string? Role { get; }

    public bool IsAuthenticated { get; }
}