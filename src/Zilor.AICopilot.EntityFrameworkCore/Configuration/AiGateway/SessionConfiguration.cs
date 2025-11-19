using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;

namespace Zilor.AICopilot.EntityFrameworkCore.Configuration.AiGateway;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        // 配置表名
        builder.ToTable("sessions");

        // 配置主键
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        // 配置外键和索引
        // 引用 ConversationTemplate 聚合
        builder.Property(s => s.TemplateId)
            .IsRequired()
            .HasColumnName("template_id");
        
        // 引用 外部的 User 聚合/实体
        builder.Property(s => s.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        // 为 UserId 创建索引，因为很可能会按用户查询会话
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("ix_sessions_user_id");
        
    }
}