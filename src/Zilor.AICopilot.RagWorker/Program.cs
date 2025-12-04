using Microsoft.Extensions.Http.Resilience;
using Zilor.AICopilot.EntityFrameworkCore;
using Zilor.AICopilot.EventBus;
using Zilor.AICopilot.Infrastructure.Storage;
using Zilor.AICopilot.RagWorker.Services;
using Zilor.AICopilot.RagWorker.Services.Embeddings;
using Zilor.AICopilot.RagWorker.Services.Parsers;
using Zilor.AICopilot.RagWorker.Services.TokenCounter;
using Zilor.AICopilot.Services.Common.Contracts;

var builder = Host.CreateApplicationBuilder(args);
// 1. 添加 Aspire 服务默认配置 (OpenTelemetry, HealthChecks 等)
builder.AddServiceDefaults();

// 2. 注册数据库上下文 (PostgreSQL)
// 这里的连接字符串名称需与 AppHost 中定义的一致
builder.AddNpgsqlDbContext<AiCopilotDbContext>("ai-copilot");

// 3. 注册文件存储服务
// 必须与 HttpApi 使用相同的存储实现，确保能读取到上传的文件
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

// 4. 注册事件总线 (RabbitMQ)
// 将自动扫描当前程序集下的 Consumer
builder.AddEventBus(typeof(Program).Assembly);

// 注册嵌入服务专用的 HttpClient
builder.Services.AddHttpClient("EmbeddingClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(20);
});

// 注册解析器
builder.Services.AddSingleton<IDocumentParser, PdfDocumentParser>();
builder.Services.AddSingleton<IDocumentParser, TextDocumentParser>();

// 注册工厂
builder.Services.AddSingleton<DocumentParserFactory>();

// 注册Token计数器
builder.Services.AddSingleton<ITokenCounter, SharpTokenCounter>();

// 注册文本分割服务
builder.Services.AddSingleton<TextSplitterService>();

// 注册嵌入生成器工厂
builder.Services.AddSingleton<EmbeddingGeneratorFactory>();

// 注册RAG服务
builder.Services.AddScoped<RagService>();


var host = builder.Build();
host.Run();