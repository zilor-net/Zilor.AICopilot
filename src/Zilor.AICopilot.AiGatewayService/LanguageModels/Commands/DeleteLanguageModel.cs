using System;
using System.Threading;
using System.Threading.Tasks;
using Zilor.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.LanguageModels.Commands;

[AuthorizeRequirement("AiGateway.DeleteLanguageModel")]
public record DeleteLanguageModelCommand(Guid Id) : ICommand<Result>;
    
public class DeleteLanguageModelCommandHandler(IRepository<LanguageModel> repo) 
    : ICommandHandler<DeleteLanguageModelCommand, Result>
{
    public async Task<Result> Handle(DeleteLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var result = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (result == null) return Result.Success();
        
        repo.Delete(result);
        await repo.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}