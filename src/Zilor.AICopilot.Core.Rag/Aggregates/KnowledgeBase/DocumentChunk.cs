using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

public class DocumentChunk : IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid DocumentId { get; set; }
    
    /// <summary>
    /// 切片在文档中的序号 (0, 1, 2...)，用于排序或重组上下文
    /// </summary>
    public int Index { get; set; }
    
    /// <summary>
    /// 切片的文本内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 外部向量数据库中的 ID (用于关联删除或更新)
    /// </summary>
    public string VectorId { get; set; } = string.Empty;
    
    // 注意：通常不建议在关系型数据库直接存储 float[] 向量数据，除非使用 pgvector 扩展。
    // 这里我们向量数据主要存储在专用的 Vector DB 中。
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public virtual Document Document { get; set; } = null!;
    
}