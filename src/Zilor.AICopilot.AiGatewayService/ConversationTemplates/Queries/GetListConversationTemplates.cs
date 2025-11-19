using Zilor.AICopilot.AiGatewayService.LanguageModels.Queries;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Contracts;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.ConversationTemplates.Queries;

public record ConversationTemplateDto
{
    public Guid Id;
    public required string Name;
    public required string Description;
    public required string SystemPrompt;
    public int? MaxTokens;
    public double? Temperature;
    public bool IsEnabled;
}

[AuthorizeRequirement("AiGateway.GetListConversationTemplates")]
public record GetListConversationTemplatesQuery : IQuery<Result<IList<ConversationTemplateDto>>>;

public class GetListConversationTemplatesQueryHandler(
    IDataQueryService dataQueryService) : IQueryHandler<GetListConversationTemplatesQuery, Result<IList<ConversationTemplateDto>>>
{
    public async Task<Result<IList<ConversationTemplateDto>>> Handle(GetListConversationTemplatesQuery request, CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.ConversationTemplates
            .Select(ct => new ConversationTemplateDto
            {
                Id = ct.Id,
                Name = ct.Name,
                Description = ct.Description,
                SystemPrompt = ct.SystemPrompt,
                MaxTokens = ct.Specification.MaxTokens,
                Temperature = ct.Specification.Temperature
            });
        var result= await dataQueryService.ToListAsync(queryable);
        return Result.Success(result);
    }
}