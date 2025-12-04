using Zilor.AICopilot.Core.Rag.Aggregates.EmbeddingModel;
using Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

namespace Zilor.AICopilot.MigrationWorkApp.SeedData;

public static class RagData
{
    private static readonly Guid[] Guids =
    [
        Guid.NewGuid()
    ];

    public static IEnumerable<EmbeddingModel> EmbeddingModels()
    {
        var item1 = new EmbeddingModel(
            "Qwen3-4b-Q8_0", 
            "Qwen", 
            "http://127.0.0.1:1234/v1",
            "text-embedding-qwen3-embedding-4b", 
            2560, 
            32 * 1000)
        {
            Id = Guids[0]
        };

        return [item1];
    }

    public static IEnumerable<KnowledgeBase> KnowledgeBases()
    {
        var item1 = new KnowledgeBase("默认知识库", "系统默认知识库", Guids[0]);
        return [item1];
    }
}