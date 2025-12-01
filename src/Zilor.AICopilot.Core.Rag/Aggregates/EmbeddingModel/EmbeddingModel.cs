using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.Rag.Aggregates.EmbeddingModel;

public class EmbeddingModel : IAggregateRoot
{
    protected EmbeddingModel()
    {
    }

    public EmbeddingModel(
        string name,
        string provider,
        string modelName,
        int dimensions,
        int maxTokens)
    {
        Id = Guid.NewGuid();
        Name = name;
        Provider = provider;
        ModelName = modelName;
        Dimensions = dimensions;
        MaxTokens = maxTokens;
        IsEnabled = true;
    }
    
    public Guid Id { get; set; }
    
    /// <summary>
    /// 显示名称 (如: "OpenAI V3 Small")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模型提供商标识 (如: "OpenAI", "AzureOpenAI", "Ollama")
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// 实际的模型标识符 (如: "text-embedding-3-small")
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// 向量维度 (如: 1536, 768, 1024)
    /// </summary>
    public int Dimensions { get; set; }

    /// <summary>
    /// 最大上下文 Token 限制 (如: 8191)
    /// 用于在分割阶段校验切片大小是否超标
    /// </summary>
    public int MaxTokens { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}