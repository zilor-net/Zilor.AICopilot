using Zilor.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.Commands;

public record CreatedLanguageModelDto(Guid Id, string Provider, string Name);

[AuthorizeRequirement("AiGateway.CreateLanguageModel")]
public record CreateLanguageModelCommand(
    string Provider, 
    string Name, 
    string BaseUrl, 
    string? ApiKey,
    int MaxTokens,
    double Temperature = 0.7) : ICommand<Result<CreatedLanguageModelDto>>;
    
public class CreateLanguageModelCommandHandler(IRepository<LanguageModel> modelRepo) 
    : ICommandHandler<CreateLanguageModelCommand, Result<CreatedLanguageModelDto>>
{
    public async Task<Result<CreatedLanguageModelDto>> Handle(CreateLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var model = new LanguageModel(
            request.Name, 
            request.Provider,
            request.BaseUrl,
            request.ApiKey,
            new ModelParameters 
            {
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature 
            });
        
        modelRepo.Add(model);

        await modelRepo.SaveChangesAsync(cancellationToken);
        
        return Result.Success(new CreatedLanguageModelDto(model.Id, model.Provider, model.Name));
    }
}