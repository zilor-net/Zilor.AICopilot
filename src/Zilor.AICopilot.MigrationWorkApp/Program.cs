using Zilor.AICopilot.EntityFrameworkCore;
using Zilor.AICopilot.MigrationWorkApp;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddEfCore();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

var host = builder.Build();
host.Run();