using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Contracts;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.LanguageModels.Queries;

public record LanguageModelDto
{
    public Guid Id;
    public required string Provider;
    public required string Name;
    public required string BaseUrl;
    public string? ApiKey;
    public int MaxTokens;
    public double Temperature;
}

[AuthorizeRequirement("AiGateway.GetListLanguageModels")]
public record GetListLanguageModelsQuery : IQuery<Result<IList<LanguageModelDto>>>;

public class GetListLanguageModelsQueryHandler(
    IDataQueryService dataQueryService) : IQueryHandler<GetListLanguageModelsQuery, Result<IList<LanguageModelDto>>>
{
    public async Task<Result<IList<LanguageModelDto>>> Handle(GetListLanguageModelsQuery request, CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.LanguageModels
            .Select(lm => new LanguageModelDto
            {
                Id = lm.Id,
                Provider = lm.Provider,
                Name = lm.Name,
                BaseUrl = lm.BaseUrl,
                ApiKey = lm.ApiKey,
                MaxTokens = lm.Parameters.MaxTokens,
                Temperature = lm.Parameters.Temperature
            });
        var result= await dataQueryService.ToListAsync(queryable);
        return Result.Success(result);
    }
}