using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zilor.AICopilot.Core.Rag.Aggregates.EmbeddingModel;

namespace Zilor.AICopilot.EntityFrameworkCore.Configuration.Rag;

public class EmbeddingModelConfiguration : IEntityTypeConfiguration<EmbeddingModel>
{
    public void Configure(EntityTypeBuilder<EmbeddingModel> builder)
    {
        builder.ToTable("embedding_models");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");
        
        // 建议添加唯一索引，防止同名模型
        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("provider");

        builder.Property(e => e.ModelName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("model_name");

        builder.Property(e => e.Dimensions)
            .IsRequired()
            .HasColumnName("dimensions");

        builder.Property(e => e.MaxTokens)
            .IsRequired()
            .HasColumnName("max_tokens");

        builder.Property(e => e.IsEnabled)
            .IsRequired()
            .HasColumnName("is_enabled");
    }
}