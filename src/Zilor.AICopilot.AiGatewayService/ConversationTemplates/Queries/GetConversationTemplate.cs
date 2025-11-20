using Zilor.AICopilot.AiGatewayService.ConversationTemplates.Dtos;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Contracts;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.ConversationTemplates.Queries;

[AuthorizeRequirement("AiGateway.GetConversationTemplate")]
public record GetConversationTemplateQuery(Guid Id) : IQuery<Result<ConversationTemplateDto>>;

public class GetConversationTemplateQueryHandler(
    IDataQueryService dataQueryService) : IQueryHandler<GetConversationTemplateQuery, Result<ConversationTemplateDto>>
{
    public async Task<Result<ConversationTemplateDto>> Handle(GetConversationTemplateQuery request, CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.ConversationTemplates
            .Where(template => template.Id == request.Id)
            .Select(ct => new ConversationTemplateDto
            {
                Id = ct.Id,
                Name = ct.Name,
                Description = ct.Description,
                SystemPrompt = ct.SystemPrompt,
                MaxTokens = ct.Specification.MaxTokens,
                Temperature = ct.Specification.Temperature
            });
        var result= await dataQueryService.FirstOrDefaultAsync(queryable);
        
        return result == null ? Result.NotFound() : Result.Success(result);
    }
}