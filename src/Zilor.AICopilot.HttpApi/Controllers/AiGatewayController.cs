using Microsoft.AspNetCore.Mvc;
using Zilor.AICopilot.AiGatewayService.LanguageModels.Commands;
using Zilor.AICopilot.AiGatewayService.LanguageModels.Queries;
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
    
    [HttpGet("language-model/list")]
    public async Task<IActionResult> GetListLanguageModels()
    {
        var result = await Sender.Send(new GetListLanguageModelsQuery());
        return ReturnResult(result);
    }
}