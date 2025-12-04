using Microsoft.Extensions.AI;
using Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

namespace Zilor.AICopilot.RagWorker.Services.VectorStorage;

public interface IVectorStorageService
{
    /// <summary>
    /// 保存文档向量到向量数据库
    /// </summary>
    /// <param name="document">文档实体（包含元数据）</param>
    /// <param name="chunks">文本切片列表</param>
    /// <param name="embeddings">对应的向量列表</param>
    /// <param name="cancellationToken"></param>
    Task SaveAsync(Document document, List<string> chunks, List<Embedding<float>> embeddings, CancellationToken cancellationToken = default);
}