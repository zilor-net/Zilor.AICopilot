using MediatR;
using Zilor.AICopilot.AiGatewayService.Queries.ConversationTemplates;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Common.Contracts;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.Commands.Sessions;

public record CreatedSessionDto(Guid Id, string Title);

[AuthorizeRequirement("AiGateway.CreateSession")]
public record CreateSessionCommand(Guid? TemplateId) : ICommand<Result<CreatedSessionDto>>;

public class CreateSessionCommandHandler(IRepository<Session> repo, IMediator mediator, ICurrentUser user)
    : ICommandHandler<CreateSessionCommand, Result<CreatedSessionDto>>
{
    public async Task<Result<CreatedSessionDto>> Handle(CreateSessionCommand request,
        CancellationToken ct)
    {
        var tempalteId = request.TemplateId;
        
        if (tempalteId == null)
        {
            var template = await mediator.Send(new GetConversationTemplateByNameQuery("GeneralAgent"), ct);
            if (!template.IsSuccess) return Result.NotFound();
            tempalteId = template.Value!.Id;
        }
        var result = new Session(new Guid(user.Id!), tempalteId.Value);

        repo.Add(result);

        await repo.SaveChangesAsync(ct);

        return Result.Success(new CreatedSessionDto(result.Id, result.Title));
    }
}