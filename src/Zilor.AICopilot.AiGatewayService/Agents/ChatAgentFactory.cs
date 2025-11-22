using System;
using System.ClientModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using OpenAI;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.AiGatewayService.Agents;

public class ChatAgentFactory(IDataQueryService data)
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
                    model.Name
                },
                Template = new
                {
                    template.Name,
                    template.SystemPrompt
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
            .CreateAIAgent(name: result.Template.Name, instructions: result.Template.SystemPrompt);

        return agent;
    }
}