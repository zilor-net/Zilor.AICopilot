using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Zilor.AICopilot.RagWorker.Models;

namespace Zilor.AICopilot.RagWorker.Services.VectorStorage;

public class QdrantVectorStorageService(
    QdrantVectorStore vectorStoreClient,
    ILogger<QdrantVectorStorageService> logger) : IVectorStorageService
{
    public async Task SaveAsync(Document document, List<string> chunks, List<Embedding<float>> embeddings, CancellationToken cancellationToken = default)
    {
        // 基础参数校验
        if (chunks.Count != embeddings.Count)
        {
            throw new ArgumentException($"切片数量 ({chunks.Count}) 与向量数量 ({embeddings.Count}) 不一致");
        }

        if (chunks.Count == 0)
        {
            logger.LogWarning("文档 {DocumentId} 没有切片需要存储", document.Id);
        }

        // 2. 确定集合名称
        // 使用 "kb-" 前缀加上知识库 ID (Guid) 作为集合名，确保名称符合 Qdrant 规范且唯一
        var collectionName = $"kb-{document.KnowledgeBaseId:N}";
        logger.LogInformation("文档 {DocumentName} 将存入集合: {CollectionName}", document.Name, collectionName);
        
        // 3. 动态获取集合实例
        // GetCollection 不会发起网络请求，它只是创建一个操作代理
        var collection = vectorStoreClient.GetCollection<ulong, VectorDocumentRecord>(collectionName);

        // 4. 确保集合存在 (Auto-Provisioning)
        // 第一次向该知识库上传文档时，会自动创建集合
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        // 5. 组装存储记录
        try
        {
            for (var i = 0; i < chunks.Count; i++)
            {
                var recordKey = (ulong)document.Id.GetHashCode() << 32 | (uint)i;

                await collection.UpsertAsync(new VectorDocumentRecord
                {
                    Key = recordKey,
                    Text = chunks[i],
                    DocumentId = document.Id.ToString(),
                    // 此时 KnowledgeBaseId 虽然在集合层面已经隐含，但保留在元数据中依然有助于调试和导出
                    KnowledgeBaseId = document.KnowledgeBaseId.ToString(),
                    ChunkIndex = i,
                    Embedding = embeddings[i].Vector
                }, cancellationToken);
            }
        
            logger.LogInformation("成功向集合 {Collection} 写入 {Count} 条向量记录。", collectionName, chunks.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "写入向量数据库失败。Collection: {Collection}", collectionName);
            throw; 
        }
        
    }
    
}