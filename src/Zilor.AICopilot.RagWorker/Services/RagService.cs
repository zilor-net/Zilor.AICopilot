
using Zilor.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Zilor.AICopilot.EntityFrameworkCore;
using Zilor.AICopilot.RagWorker.Services.Parsers;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.RagWorker.Services;

public class RagService(
    IFileStorageService fileStorage,
    DocumentParserFactory parserFactory,
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
        // TODO: 分割

        // --- Step 4: 嵌入  ---
        // TODO: 嵌入

        // --- Step 5: 存储  ---
        // TODO: 存储
        
        // 标记完成，流程还未全部实现，先不做标记
        // document.MarkAsIndexed();
        // await dbContext.SaveChangesAsync(cancellationToken);
    }
}