using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

public class Document : IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid KnowledgeBaseId { get; set; }
    
    /// <summary>
    /// 原始文件名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 文件存储路径 (如 Blob Storage 地址 或 本地路径)
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 文件类型扩展名 (.pdf, .md, .txt)
    /// </summary>
    public string Extension { get; set; } = string.Empty;
    
    /// <summary>
    /// 文件内容哈希值 (MD5/SHA256)。用于检测文件变动，实现增量更新或幂等性。
    /// </summary>
    public string FileHash { get; set; } = string.Empty;
    
    /// <summary>
    /// 文档处理状态
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    
    /// <summary>
    /// 切片数量
    /// </summary>
    public int ChunkCount { get; set; }
    
    /// <summary>
    /// 处理过程中的错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    // 导航属性
    public virtual KnowledgeBase KnowledgeBase { get; set; } = null!;
    public virtual ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}

public enum DocumentStatus
{
    Pending = 0,      // 等待处理
    Parsing = 1,      // 正在解析/读取
    Splitting = 2,    // 正在切片
    Embedding = 3,    // 正在向量化
    Indexed = 4,      // 索引完成
    Failed = 5        // 处理失败
}