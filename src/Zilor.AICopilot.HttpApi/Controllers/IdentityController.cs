using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zilor.AICopilot.HttpApi.Infrastructure;
using Zilor.AICopilot.HttpApi.Models;
using Zilor.AICopilot.IdentityService.Commands;

namespace Zilor.AICopilot.HttpApi.Controllers;

[Route("/api/identity")]
public class IdentityController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterRequest request)
    {
        var result = await Sender.Send(new CreateUserCommand(request.Username, request.Password));

        return ReturnResult(result);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginRequest request)
    {
        var result = await Sender.Send(new LoginUserCommand(request.Username, request.Password));
        return ReturnResult(result);
    }
    
    [Authorize]
    [HttpPost("test")]
    public IActionResult Test()
    {
        return Ok(new
        {
            Username = User.FindFirstValue(ClaimTypes.Name)
        });
    }
}