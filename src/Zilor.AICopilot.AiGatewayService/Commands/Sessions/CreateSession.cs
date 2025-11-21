using System;
using System.Threading;
using System.Threading.Tasks;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Common.Contracts;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.Commands.Sessions;

public record CreatedSessionDto(Guid Id);

[AuthorizeRequirement("AiGateway.CreateSession")]
public record CreateSessionCommand(Guid TemplateId) : ICommand<Result<CreatedSessionDto>>;

public class CreateSessionCommandHandler(IRepository<Session> repo, ICurrentUser user)
    : ICommandHandler<CreateSessionCommand, Result<CreatedSessionDto>>
{
    public async Task<Result<CreatedSessionDto>> Handle(CreateSessionCommand request,
        CancellationToken cancellationToken)
    {
        var result = new Session(new Guid(user.Id!), request.TemplateId);

        repo.Add(result);

        await repo.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatedSessionDto(result.Id));
    }
}