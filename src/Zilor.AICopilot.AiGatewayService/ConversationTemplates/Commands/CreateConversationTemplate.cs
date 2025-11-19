using Zilor.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.ConversationTemplates.Commands;

public record CreatedConversationTemplateDto(Guid Id, string Name);

[AuthorizeRequirement("AiGateway.CreateConversationTemplate")]
public record CreateConversationTemplateCommand(
    string Name, 
    string Description, 
    string SystemPrompt, 
    int? MaxTokens,
    double? Temperature) : ICommand<Result<CreatedConversationTemplateDto>>;
    
public class CreateConversationTemplateCommandHandler(IRepository<ConversationTemplate> repo) 
    : ICommandHandler<CreateConversationTemplateCommand, Result<CreatedConversationTemplateDto>>
{
    public async Task<Result<CreatedConversationTemplateDto>> Handle(CreateConversationTemplateCommand request, CancellationToken cancellationToken)
    {
        var model = new ConversationTemplate(
            request.Name, 
            request.Description,
            request.SystemPrompt,
            new TemplateSpecification 
            {
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature 
            });
        
        repo.Add(model);

        await repo.SaveChangesAsync(cancellationToken);
        
        return Result.Success(new CreatedConversationTemplateDto(model.Id, model.Name));
    }
}