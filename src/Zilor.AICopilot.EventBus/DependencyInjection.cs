using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Zilor.AICopilot.EventBus;

public static class DependencyInjection
{
    public static void AddEventBus(this IHostApplicationBuilder builder) 
    {
        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // 默认配置 RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                // 从 Aspire 注入的连接字符串中读取配置
                // 连接字符串名必须与 AppHost 中 .AddRabbitMQ("eventbus") 的名称一致
                var connectionString = builder.Configuration.GetConnectionString("eventbus");
                cfg.Host(connectionString);
                
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}