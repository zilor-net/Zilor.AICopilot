using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Zilor.AICopilot.HttpApi;
using Zilor.AICopilot.Infrastructure;

var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? nameof(Zilor.AICopilot.HttpApi);
var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";
            
var resource = ResourceBuilder.CreateDefault()
    .AddService(serviceName);
            
// Setup tracing with resource
Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resource)
    .AddSource(nameof(Zilor.AICopilot.AiGatewayService))
    .AddSource("*Microsoft.Agents.AI*")
    .AddSource("*Microsoft.Extensions.AI*")
    .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint))
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddInfrastructures();
builder.AddApplicationService();
builder.AddWebServices();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();