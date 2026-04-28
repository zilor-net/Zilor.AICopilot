# Zilor.AICopilot

[简体中文](README.zh-CN.md)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![Aspire](https://img.shields.io/badge/.NET%20Aspire-enabled-512BD4)
![Vue](https://img.shields.io/badge/Vue-3.x-42b883)
![Vite](https://img.shields.io/badge/Vite-7.x-646CFF)
![License](https://img.shields.io/badge/license-not%20specified-lightgrey)

Zilor.AICopilot is a practical enterprise AI assistant built with .NET 10, .NET Aspire, Microsoft Agent Framework, RAG, Text2SQL, MCP, and generative UI patterns. The project demonstrates how to structure, orchestrate, and deliver a production-oriented AI application using DDD, CQRS, Clean Architecture, distributed resources, identity, observability, and container deployment.

## What This Project Covers

- Enterprise architecture with DDD, CQRS, Clean Architecture, MediatR, EF Core, JWT authentication, and declarative authorization.
- .NET Aspire distributed orchestration for PostgreSQL, RabbitMQ, Qdrant, API services, workers, migrations, and the Vue Web UI.
- Microsoft Agent Framework integration for chat, short-term memory, function calling, tool assembly, OpenAI client optimization, and tracing.
- Intent recognition and plugin routing, including classification agents, workflow execution, and extensible agent plugins.
- RAG and Agentic RAG, including knowledge bases, file ingestion, document parsing, chunking, dynamic embeddings, vector storage, retrieval, and parallel cognitive flows.
- Text2SQL and dynamic data analysis agents, including multi-source data access, ReAct-style reasoning, DBA expert agents, and analysis executors.
- Generative UI with streaming protocols, visualization context, dual-channel output, frontend state management, and widget rendering.
- MCP protocol integration, sensitive operation approval, human-in-the-loop workflows, and Docker Compose deployment.

## Architecture

```text
Vue Web UI + Generative UI Widgets
        |
        v
HttpApi -- IdentityService
   |  \-- AiGatewayService -- Microsoft Agent Framework
   |       |-- Intent Workflow
   |       |-- Plugin System / MCP Plugins
   |       |-- Tool Approval
   |
   |  \-- RagService -------> Qdrant
   |  \-- DataAnalysisService / Text2SQL
   |
PostgreSQL + EF Core + Dapper
RabbitMQ EventBus
RagWorker
Aspire AppHost / Docker Compose
```

`Zilor.AICopilot.AppHost` is the recommended local entry point. It orchestrates PostgreSQL, RabbitMQ, Qdrant, the migration app, HTTP API, RAG worker, and frontend.

## Tech Stack

| Area | Technology |
| --- | --- |
| Backend | .NET 10, ASP.NET Core, MediatR, CQRS |
| Architecture | DDD, Clean Architecture, distributed services |
| Orchestration | .NET Aspire, Docker Compose |
| Data | PostgreSQL, EF Core, Dapper |
| Messaging | RabbitMQ, MassTransit |
| AI Agents | Microsoft Agent Framework, Microsoft.Extensions.AI, Semantic Kernel |
| RAG | Qdrant, embeddings, document parsing, chunking |
| Frontend | Vue 3, Vite, TypeScript, Pinia, Element Plus, ECharts |
| Observability | OpenTelemetry, Aspire Dashboard |

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop or compatible Docker runtime
- Node.js `^20.19.0` or `>=22.12.0`
- npm

### Run the Aspire Stack

```powershell
dotnet restore Zilor.AICopilot.sln
dotnet build Zilor.AICopilot.sln
dotnet run --project src/Zilor.AICopilot.AppHost
```

Use the Aspire Dashboard to inspect service endpoints, logs, traces, and resource health. In development, the HTTP API exposes OpenAPI and Swagger UI.

### Run the Frontend Only

```powershell
cd src/Zilor.AICopilot.Web
npm install
npm run dev
```

## Project Structure

```text
.
├── Zilor.AICopilot.sln
├── AGENTS.md
├── artifacts/
│   ├── docker-compose.yaml
│   └── sql/
└── src/
    ├── Zilor.AICopilot.AppHost/
    ├── Zilor.AICopilot.HttpApi/
    ├── Zilor.AICopilot.AiGatewayService/
    ├── Zilor.AICopilot.RagService/
    ├── Zilor.AICopilot.RagWorker/
    ├── Zilor.AICopilot.DataAnalysisService/
    ├── Zilor.AICopilot.McpService/
    ├── Zilor.AICopilot.IdentityService/
    ├── Zilor.AICopilot.EntityFrameworkCore/
    ├── Zilor.AICopilot.Core.*/
    └── Zilor.AICopilot.Web/
```

See [`AGENTS.md`](AGENTS.md) for contributor guidance.

## Learning and Implementation Roadmap

1. Build the enterprise foundation: DDD, CQRS, Clean Architecture, Aspire orchestration, identity, authorization, and model/session/template management.
2. Integrate Microsoft Agent Framework: chat use cases, memory, function calling, tool assembly, OpenAI client pooling, and observability.
3. Add intent recognition and plugin workflows: classification prompts, intent agents, workflow routing, and extensible plugins.
4. Implement RAG: knowledge bases, file storage, document parsing, text splitting, embeddings, Qdrant storage, and retrieval services.
5. Extend to Agentic RAG: dual-intent prompts, parallel cognitive flows, retrieval executors, normalization, and aggregation.
6. Build Text2SQL agents: dynamic data sources, ReAct reasoning, DBA expert agents, analysis executors, and test databases.
7. Deliver generative UI: visualization protocols, streaming extensions, frontend architecture, chat UI, and widget rendering.
8. Integrate MCP and approval flows: MCP host/plugins, sensitive tool interception, approval workflows, and approval cards.
9. Package and deploy: Aspire deployment flow, Docker Compose generation, .NET native containers, and frontend container images.

## Development Commands

| Command | Description |
| --- | --- |
| `dotnet restore Zilor.AICopilot.sln` | Restore NuGet packages |
| `dotnet build Zilor.AICopilot.sln` | Build all .NET projects |
| `dotnet run --project src/Zilor.AICopilot.AppHost` | Run the full Aspire stack |
| `dotnet run --project src/Zilor.AICopilot.HttpApi` | Run the API service only |
| `cd src/Zilor.AICopilot.Web && npm run dev` | Start the Vite dev server |
| `cd src/Zilor.AICopilot.Web && npm run build` | Type-check and build the frontend |
| `cd src/Zilor.AICopilot.Web && npm run lint` | Run ESLint with fixes |
| `dotnet test` | Run tests when test projects are added |

## API Overview

- `POST /api/identity/register`
- `POST /api/identity/login`
- `POST /api/aigateway/language-model`
- `GET /api/aigateway/session/list`
- `POST /api/aigateway/chat`
- `POST /api/rag/knowledge-base`
- `POST /api/rag/document`
- `POST /api/rag/search`

Most AI gateway and RAG endpoints require authentication. Obtain a token from the identity APIs before calling protected resources.

## Configuration and Security

Local orchestration is configured in `src/Zilor.AICopilot.AppHost` and `artifacts/`. `artifacts/docker-compose.yaml` defines PostgreSQL, RabbitMQ, Qdrant, the API, worker, and Web UI containers.

Do not commit production secrets. Replace `appsettings.Development.json`, Aspire settings, `.env`, JWT secrets, connection strings, model provider keys, and frontend debug tokens per environment.

## Contributing

Before submitting changes, run the relevant build, formatting, and validation commands. Keep changes focused and describe the summary, affected modules, configuration changes, migration impact, and verification results in the PR. Include screenshots or recordings for UI changes.

## License

This repository does not currently include a license file. Confirm project authorization before use, distribution, or commercial deployment.
