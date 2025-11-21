using Microsoft.EntityFrameworkCore;
using Zilor.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;
using Zilor.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.EntityFrameworkCore;

public class DataQueryService(AiCopilotDbContext dbContext) : IDataQueryService
{
    public IQueryable<ConversationTemplate> ConversationTemplates => dbContext.ConversationTemplates.AsNoTracking();
    public IQueryable<LanguageModel> LanguageModels => dbContext.LanguageModels.AsNoTracking();
    public IQueryable<Session> Sessions => dbContext.Sessions.AsNoTracking();
    public IQueryable<Message> Messages => dbContext.Messages.AsNoTracking();

    public async Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable) where T : class
    {
        return await queryable.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<IList<T>> ToListAsync<T>(IQueryable<T> queryable) where T : class
    {
        return await queryable.AsNoTracking().ToListAsync();
    }

    public async Task<bool> AnyAsync<T>(IQueryable<T> queryable) where T : class
    {
        return await queryable.AsNoTracking().AnyAsync();
    }
}