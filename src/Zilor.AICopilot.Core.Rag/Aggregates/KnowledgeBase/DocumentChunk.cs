using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

public class DocumentChunk : IEntity<Guid>
{
    protected DocumentChunk()
    {
    }

    internal DocumentChunk(Guid documentId, int index, string content)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        Index = index;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    
    public Guid DocumentId { get; private set; }
    
    /// <summary>
    /// 切片序号
    /// </summary>
    public int Index { get; private set; }
    
    /// <summary>
    /// 文本内容
    /// </summary>
    public string Content { get; private set; } = string.Empty;
    
    /// <summary>
    /// 向量数据库中的ID
    /// </summary>
    public string? VectorId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    // 导航属性
    public Document Document { get; private set; } = null!;

    /// <summary>
    /// 设置向量ID (当向量化完成后调用)
    /// </summary>
    public void SetVectorId(string vectorId)
    {
        VectorId = vectorId;
    }
}