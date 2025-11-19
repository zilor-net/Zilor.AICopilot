using Zilor.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;
using Zilor.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;

namespace Zilor.AICopilot.Services.Contracts;

public interface IDataQueryService
{
    public IQueryable<ConversationTemplate> ConversationTemplates { get; }

    public IQueryable<LanguageModel> LanguageModels { get; }

    public IQueryable<Session> Sessions { get; }

    public IQueryable<Message> Messages { get; }

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable) where T : class;

    Task<IList<T>> ToListAsync<T>(IQueryable<T> queryable) where T : class;

    Task<bool> AnyAsync<T>(IQueryable<T> queryable) where T : class;
}
