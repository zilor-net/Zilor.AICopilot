using Microsoft.AspNetCore.Mvc;
using Zilor.AICopilot.AiGatewayService.Commands;
using Zilor.AICopilot.HttpApi.Infrastructure;

namespace Zilor.AICopilot.HttpApi.Controllers;

[Route("/api/aigateway")]
public class AiGatewayController : ApiControllerBase
{
    [HttpPost("language-model")]
    public async Task<IActionResult> CreateLanguageModel(CreateLanguageModelCommand command)
    {
        var result = await Sender.Send(command);

        return ReturnResult(result);
    }
    
    [HttpDelete("language-model")]
    public async Task<IActionResult> DeleteLanguageModel(DeleteLanguageModelCommand command)
    {
        var result = await Sender.Send(command);

        return ReturnResult(result);
    }
}