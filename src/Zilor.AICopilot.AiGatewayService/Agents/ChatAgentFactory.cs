using System;
using System.ClientModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.AiGatewayService.Agents;

public class ChatAgentFactory(IDataQueryService data, IServiceProvider serviceProvider)
{
    public async Task<ChatClientAgent> CreateAgentAsync(Guid templateId)
    {
        var queryable =
            from template in data.ConversationTemplates
            join model in data.LanguageModels on template.ModelId equals model.Id
            where template.Id == templateId
            select new
            {
                Model = new
                {
                    model.BaseUrl,
                    model.ApiKey,
                    model.Name,
                    model.Parameters.Temperature
                },
                Template = new
                {
                    template.Name,
                    template.SystemPrompt,
                    template.Specification.Temperature
                }
            };

        var result = await data.FirstOrDefaultAsync(queryable);
        if (result == null) throw new Exception("未找对话模板或模型");
        
        var agent = new OpenAIClient(
                new ApiKeyCredential(result.Model.ApiKey), 
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(result.Model.BaseUrl)
                })
            .GetChatClient(result.Model.Name)
            .CreateAIAgent(new ChatClientAgentOptions
            {
                Name = result.Template.Name,
                Instructions = result.Template.SystemPrompt,
                ChatOptions = new ChatOptions
                {
                    Temperature = result.Template.Temperature ?? result.Model.Temperature
                },
                ChatMessageStoreFactory = context => new SessionChatMessageStore(serviceProvider, context.SerializedState)
            });

        return agent;
    }
}