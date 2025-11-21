using Microsoft.AspNetCore.Mvc;
using Zilor.AICopilot.AiGatewayService.UseCases.ConversationTemplates.Commands;
using Zilor.AICopilot.AiGatewayService.UseCases.ConversationTemplates.Queries;
using Zilor.AICopilot.AiGatewayService.UseCases.LanguageModels.Commands;
using Zilor.AICopilot.AiGatewayService.UseCases.LanguageModels.Queries;
using Zilor.AICopilot.AiGatewayService.UseCases.Sessions.Commands;
using Zilor.AICopilot.AiGatewayService.UseCases.Sessions.Queries;
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

    [HttpPost("conversation-template")]
    public async Task<IActionResult> CreateConversationTemplate(CreateConversationTemplateCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    [HttpDelete("conversation-template")]
    public async Task<IActionResult> DeleteConversationTemplate(DeleteConversationTemplateCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    [HttpGet("conversation-template")]
    public async Task<IActionResult> GetConversationTemplate(GetConversationTemplateQuery query)
    {
        var result = await Sender.Send(query);
        return ReturnResult(result);
    }

    [HttpGet("conversation-template/list")]
    public async Task<IActionResult> GetListConversationTemplates()
    {
        var result = await Sender.Send(new GetListConversationTemplatesQuery());
        return ReturnResult(result);
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateSession(CreateSessionCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    [HttpDelete("session")]
    public async Task<IActionResult> DeleteSession(DeleteSessionCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    [HttpGet("session/list")]
    public async Task<IActionResult> GetListSessions()
    {
        var result = await Sender.Send(new GetListSessionsQuery());
        return ReturnResult(result);
    }
}