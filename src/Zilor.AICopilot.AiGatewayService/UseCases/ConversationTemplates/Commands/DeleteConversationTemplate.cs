using System;
using System.Threading;
using System.Threading.Tasks;
using Zilor.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.UseCases.ConversationTemplates.Commands;

[AuthorizeRequirement("AiGateway.DeleteConversationTemplate")]
public record DeleteConversationTemplateCommand(Guid Id) : ICommand<Result>;

public class DeleteConversationTemplateCommandHandler(IRepository<ConversationTemplate> modelRepo)
    : ICommandHandler<DeleteConversationTemplateCommand, Result>
{
    public async Task<Result> Handle(DeleteConversationTemplateCommand request, CancellationToken cancellationToken)
    {
        var model = await modelRepo.GetByIdAsync(request.Id, cancellationToken);
        if (model == null) return Result.Success();

        modelRepo.Delete(model);
        await modelRepo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}