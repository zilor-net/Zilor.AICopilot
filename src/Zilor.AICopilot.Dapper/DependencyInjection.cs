using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zilor.AICopilot.Dapper.Security;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.Dapper;

public static class DependencyInjection
{
    public static void AddInfrastructures(this IHostApplicationBuilder builder)
    {
        // 注册 SQL 安全服务
        builder.Services.AddSingleton<ISqlGuardrail, KeywordSqlGuardrail>();
        
        // 注册 数据库连接器
        builder.Services.AddScoped<IDatabaseConnector, DapperDatabaseConnector>();
    }
}