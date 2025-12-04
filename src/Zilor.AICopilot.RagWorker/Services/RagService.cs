
using Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Zilor.AICopilot.EntityFrameworkCore;
using Zilor.AICopilot.RagWorker.Services.Parsers;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.RagWorker.Services;

public class RagService(
    IFileStorageService fileStorage,
    DocumentParserFactory parserFactory,
    TextSplitterService textSplitter,
    AiCopilotDbContext dbContext,
    ILogger<RagService> logger)
{
    public async Task IndexDocumentAsync(Document document, CancellationToken cancellationToken = new())
    {
        logger.LogInformation("开始索引流程: {DocumentName}", document.Name);

        // --- Step 1: 加载 ---
        // 从存储中获取文件流
        await using var stream = await fileStorage.GetAsync(document.FilePath, cancellationToken);
        if (stream == null)
        {
            throw new FileNotFoundException($"文件未找到: {document.FilePath}");
        }

        // --- Step 2: 解析 ---
        // 根据扩展名获取解析器
        var parser = parserFactory.GetParser(document.Extension);
        
        // 提取文本
        var text = await parser.ParseAsync(stream, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("文档内容为空或无法提取文本。");
        }
        
        logger.LogInformation("文本提取完成，长度: {Length} 字符", text.Length);
        
        // 更新状态：解析完成 -> 准备切片
        document.CompleteParsing();
        await dbContext.SaveChangesAsync(cancellationToken);

        // --- Step 3: 分割---
        logger.LogInformation("开始文本切片...");
        
        // 为了支持重新索引，如果文档之前处理过，需要先清理旧的切片
        if (document.Chunks.Count != 0)
        {
            document.ClearChunks();
        }

        // 调用 SK TextChunker 逻辑
        var paragraphs = textSplitter.Split(text);
        
        logger.LogInformation("文本切片完成，共生成 {Count} 个切片。", paragraphs.Count);

        // 将切片转换为领域实体
        for (var i = 0; i < paragraphs.Count; i++)
        {
            // AddChunk 是我们在 Domain 层定义的行为方法
            document.AddChunk(i, paragraphs[i]);
        }

        // 保存切片到数据库 (PostgreSQL)
        // 这一步很重要，我们在进行向量化之前，先持久化切片内容
        // 这样即使后续向量化失败，我们也不需要重新解析 PDF
        await dbContext.SaveChangesAsync(cancellationToken);

        // --- Step 4: 嵌入  ---
        // TODO: 嵌入

        // --- Step 5: 存储  ---
        // TODO: 存储
        
        // 标记完成，流程还未全部实现，先不做标记
        // document.MarkAsIndexed();
        // await dbContext.SaveChangesAsync(cancellationToken);
    }
}