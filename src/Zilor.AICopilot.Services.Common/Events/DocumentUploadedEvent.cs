using System;

namespace Zilor.AICopilot.Services.Common.Events;

public record DocumentUploadedEvent
{
    public Guid DocumentId { get; init; }
    public Guid KnowledgeBaseId { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}