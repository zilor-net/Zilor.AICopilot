using System.Reflection.Metadata;
using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

public class KnowledgeBase : IAggregateRoot
{
    protected KnowledgeBase()
    {
    }
    
    public KnowledgeBase(string name, string description, Guid embeddingModelId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        EmbeddingModelId = embeddingModelId;
    }
    
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 嵌入模型ID。一个知识库内的所有文档必须使用相同的嵌入模型，否则向量空间不兼容。
    /// </summary>
    public Guid EmbeddingModelId { get; set; }
    
    // 导航属性
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}