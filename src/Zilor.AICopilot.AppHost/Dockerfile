FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Zilor.AICopilot.AppHost/Zilor.AICopilot.AppHost.csproj", "src/Zilor.AICopilot.AppHost/"]
COPY ["src/Zilor.AICopilot.HttpApi/Zilor.AICopilot.HttpApi.csproj", "src/Zilor.AICopilot.HttpApi/"]
COPY ["src/Zilor.AICopilot.SharedKernel/Zilor.AICopilot.SharedKernel.csproj", "src/Zilor.AICopilot.SharedKernel/"]
COPY ["src/Zilor.AICopilot.Services.Common/Zilor.AICopilot.Services.Common.csproj", "src/Zilor.AICopilot.Services.Common/"]
COPY ["src/Zilor.AICopilot.Core.AiGateway/Zilor.AICopilot.Core.AiGateway.csproj", "src/Zilor.AICopilot.Core.AiGateway/"]
COPY ["src/Zilor.AICopilot.Core.Rag/Zilor.AICopilot.Core.Rag.csproj", "src/Zilor.AICopilot.Core.Rag/"]
COPY ["src/Zilor.AICopilot.Core.DataAnalysis/Zilor.AICopilot.Core.DataAnalysis.csproj", "src/Zilor.AICopilot.Core.DataAnalysis/"]
COPY ["src/Zilor.AICopilot.Core.McpServer/Zilor.AICopilot.Core.McpServer.csproj", "src/Zilor.AICopilot.Core.McpServer/"]
COPY ["src/Zilor.AICopilot.EntityFrameworkCore/Zilor.AICopilot.EntityFrameworkCore.csproj", "src/Zilor.AICopilot.EntityFrameworkCore/"]
COPY ["src/Zilor.AICopilot.IdentityService/Zilor.AICopilot.IdentityService.csproj", "src/Zilor.AICopilot.IdentityService/"]
COPY ["src/Zilor.AICopilot.ServiceDefaults/Zilor.AICopilot.ServiceDefaults.csproj", "src/Zilor.AICopilot.ServiceDefaults/"]
COPY ["src/Zilor.AICopilot.AiGatewayService/Zilor.AICopilot.AiGatewayService.csproj", "src/Zilor.AICopilot.AiGatewayService/"]
COPY ["src/Zilor.AICopilot.AgentPlugin/Zilor.AICopilot.AgentPlugin.csproj", "src/Zilor.AICopilot.AgentPlugin/"]
COPY ["src/Zilor.AICopilot.EventBus/Zilor.AICopilot.EventBus.csproj", "src/Zilor.AICopilot.EventBus/"]
COPY ["src/Zilor.AICopilot.Embedding/Zilor.AICopilot.Embedding.csproj", "src/Zilor.AICopilot.Embedding/"]
COPY ["src/Zilor.AICopilot.Dapper/Zilor.AICopilot.Dapper.csproj", "src/Zilor.AICopilot.Dapper/"]
COPY ["src/Zilor.AICopilot.RagService/Zilor.AICopilot.RagService.csproj", "src/Zilor.AICopilot.RagService/"]
COPY ["src/Zilor.AICopilot.DataAnalysisService/Zilor.AICopilot.DataAnalysisService.csproj", "src/Zilor.AICopilot.DataAnalysisService/"]
COPY ["src/Zilor.AICopilot.Visualization/Zilor.AICopilot.Visualization.csproj", "src/Zilor.AICopilot.Visualization/"]
COPY ["src/Zilor.AICopilot.Infrastructure/Zilor.AICopilot.Infrastructure.csproj", "src/Zilor.AICopilot.Infrastructure/"]
COPY ["src/Zilor.AICopilot.McpService/Zilor.AICopilot.McpService.csproj", "src/Zilor.AICopilot.McpService/"]
COPY ["src/Zilor.AICopilot.MigrationWorkApp/Zilor.AICopilot.MigrationWorkApp.csproj", "src/Zilor.AICopilot.MigrationWorkApp/"]
COPY ["src/Zilor.AICopilot.RagWorker/Zilor.AICopilot.RagWorker.csproj", "src/Zilor.AICopilot.RagWorker/"]
RUN dotnet restore "src/Zilor.AICopilot.AppHost/Zilor.AICopilot.AppHost.csproj"
COPY . .
WORKDIR "/src/src/Zilor.AICopilot.AppHost"
RUN dotnet build "./Zilor.AICopilot.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Zilor.AICopilot.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zilor.AICopilot.AppHost.dll"]
