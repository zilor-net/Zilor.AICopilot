using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Contracts;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.Sessions.Queries;

public record SessionDto
{
    public Guid Id;
    public required string Title;
}

[AuthorizeRequirement("AiGateway.GetListSessions")]
public record GetListSessionsQuery : IQuery<Result<IList<SessionDto>>>;

public class GetListSessionsQueryHandler(
    IDataQueryService dataQueryService) : IQueryHandler<GetListSessionsQuery, Result<IList<SessionDto>>>
{
    public async Task<Result<IList<SessionDto>>> Handle(GetListSessionsQuery request, CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.Sessions
            .Select(s => new SessionDto
            {
                Id = s.Id,
                Title = s.Title
            });
        var result= await dataQueryService.ToListAsync(queryable);
        return Result.Success(result);
    }
}