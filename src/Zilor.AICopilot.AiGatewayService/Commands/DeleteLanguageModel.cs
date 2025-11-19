using Zilor.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.Commands;

[AuthorizeRequirement("AiGateway.DeleteLanguageModel")]
public record DeleteLanguageModelCommand(Guid Id) : ICommand<Result>;
    
public class DeleteLanguageModelCommandHandler(IRepository<LanguageModel> modelRepo) 
    : ICommandHandler<DeleteLanguageModelCommand, Result>
{
    public async Task<Result> Handle(DeleteLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var model = await modelRepo.GetByIdAsync(request.Id, cancellationToken);
        if (model == null) return Result.Success();
        
        modelRepo.Delete(model);
        await modelRepo.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}