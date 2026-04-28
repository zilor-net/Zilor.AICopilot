using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.AI;
using OpenAI;
using Zilor.AICopilot.Core.Rag.Aggregates.EmbeddingModel;

namespace Zilor.AICopilot.Embedding;

public class EmbeddingGeneratorFactory(IHttpClientFactory httpClientFactory)
{
    public IEmbeddingGenerator<string, Embedding<float>> CreateGenerator(EmbeddingModel model)
    {
        var httpClient = httpClientFactory.CreateClient("EmbeddingClient");

        // 创建 OpenAI 嵌入客户端
        var client = new OpenAIClient(
            new ApiKeyCredential(model.ApiKey ?? "empty"),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(model.BaseUrl),
                Transport = new HttpClientPipelineTransport(httpClient)
            }).GetEmbeddingClient(model.ModelName);
        
        return client.AsIEmbeddingGenerator(model.Dimensions);
    }
}