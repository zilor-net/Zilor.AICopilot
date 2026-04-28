# Zilor.AICopilot

[English](README.md)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![Aspire](https://img.shields.io/badge/.NET%20Aspire-enabled-512BD4)
![Vue](https://img.shields.io/badge/Vue-3.x-42b883)
![Vite](https://img.shields.io/badge/Vite-7.x-646CFF)
![License](https://img.shields.io/badge/license-not%20specified-lightgrey)

Zilor.AICopilot 是一个面向企业 AI 助手场景的 .NET AI 实战项目，基于 .NET 10、.NET Aspire、微软 Agent 框架、RAG、Text2SQL、MCP 与生成式 UI 构建。项目展示如何使用 DDD、CQRS、整洁架构、分布式资源编排、身份认证、可观测性和容器化部署，构建具备工程化能力的 AI 应用。

## 项目覆盖内容

- 企业级架构：DDD、CQRS、整洁架构、MediatR、EF Core、JWT 认证和声明式授权。
- .NET Aspire 分布式编排：PostgreSQL、RabbitMQ、Qdrant、API 服务、Worker、迁移应用和 Vue Web UI。
- 微软 Agent 框架集成：对话、短期记忆、函数调用、工具装配、OpenAI 客户端优化和链路追踪。
- 意图识别与插件路由：分类 Agent、工作流执行和可扩展 Agent 插件系统。
- RAG 与 Agentic RAG：知识库、文件导入、文档解析、文本分割、动态嵌入、向量存储、检索和并行认知流。
- Text2SQL 与动态数据分析 Agent：多数据源访问、ReAct 推理、DBA 专家 Agent 和数据分析执行器。
- 生成式 UI：流式协议、可视化上下文、双路输出、前端状态管理和组件渲染。
- MCP 协议集成、敏感操作审批、人机协作工作流和 Docker Compose 部署。

## 架构概览

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

`Zilor.AICopilot.AppHost` 是本地开发的推荐入口，负责编排 PostgreSQL、RabbitMQ、Qdrant、迁移应用、HTTP API、RAG Worker 和前端应用。

## 技术栈

| 领域 | 技术 |
| --- | --- |
| 后端 | .NET 10, ASP.NET Core, MediatR, CQRS |
| 架构 | DDD, 整洁架构, 分布式服务 |
| 编排 | .NET Aspire, Docker Compose |
| 数据 | PostgreSQL, EF Core, Dapper |
| 消息 | RabbitMQ, MassTransit |
| AI Agent | Microsoft Agent Framework, Microsoft.Extensions.AI, Semantic Kernel |
| RAG | Qdrant, 嵌入模型, 文档解析, 文本分割 |
| 前端 | Vue 3, Vite, TypeScript, Pinia, Element Plus, ECharts |
| 可观测性 | OpenTelemetry, Aspire Dashboard |

## 快速开始

### 环境要求

- .NET 10 SDK
- Docker Desktop 或兼容 Docker 的运行时
- Node.js `^20.19.0` 或 `>=22.12.0`
- npm

### 使用 Aspire 启动

```powershell
dotnet restore Zilor.AICopilot.sln
dotnet build Zilor.AICopilot.sln
dotnet run --project src/Zilor.AICopilot.AppHost
```

启动后可在 Aspire Dashboard 中查看服务地址、日志、链路追踪和资源状态。开发环境下 HTTP API 会启用 OpenAPI 与 Swagger UI。

### 单独启动前端

```powershell
cd src/Zilor.AICopilot.Web
npm install
npm run dev
```

## 项目结构

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

贡献者指南见 [`AGENTS.md`](AGENTS.md)。

## 学习与实现路线

1. 构建企业级基础：DDD、CQRS、整洁架构、Aspire 编排、身份系统、授权、模型/会话/模板管理。
2. 集成微软 Agent 框架：对话用例、记忆、函数调用、工具装配、OpenAI 客户端连接池和可观测性。
3. 增加意图识别与插件工作流：分类提示词、意图分类 Agent、工作流路由和可扩展插件。
4. 实现 RAG：知识库、文件存储、文档解析、文本分割、嵌入、Qdrant 存储和检索服务。
5. 扩展 Agentic RAG：双重意图提示、并行认知流、知识检索执行器、归一化和聚合。
6. 构建 Text2SQL Agent：动态数据源、ReAct 推理、DBA 专家 Agent、分析执行器和测试数据库。
7. 交付生成式 UI：可视化协议、流式扩展、前端架构、聊天界面和组件渲染。
8. 集成 MCP 与审批流：MCP 宿主/插件、敏感工具拦截、审批工作流和审批卡片。
9. 容器化部署：Aspire 部署流程、Docker Compose 生成、.NET 原生容器和前端镜像。

## 开发命令

| 命令 | 说明 |
| --- | --- |
| `dotnet restore Zilor.AICopilot.sln` | 还原 NuGet 包 |
| `dotnet build Zilor.AICopilot.sln` | 构建所有 .NET 项目 |
| `dotnet run --project src/Zilor.AICopilot.AppHost` | 启动完整 Aspire 本地环境 |
| `dotnet run --project src/Zilor.AICopilot.HttpApi` | 仅启动 API 服务 |
| `cd src/Zilor.AICopilot.Web && npm run dev` | 启动 Vite 前端开发服务器 |
| `cd src/Zilor.AICopilot.Web && npm run build` | 类型检查并构建前端 |
| `cd src/Zilor.AICopilot.Web && npm run lint` | 运行 ESLint 自动修复 |
| `dotnet test` | 添加测试项目后运行测试 |

## API 概览

- `POST /api/identity/register`
- `POST /api/identity/login`
- `POST /api/aigateway/language-model`
- `GET /api/aigateway/session/list`
- `POST /api/aigateway/chat`
- `POST /api/rag/knowledge-base`
- `POST /api/rag/document`
- `POST /api/rag/search`

多数 AI 网关与 RAG 接口需要认证，请先通过身份接口获取 token。

## 配置与安全

本地编排配置位于 `src/Zilor.AICopilot.AppHost` 和 `artifacts/`。`artifacts/docker-compose.yaml` 定义了 PostgreSQL、RabbitMQ、Qdrant、API、Worker 和 Web UI 容器。

不要将生产密钥提交到仓库。`appsettings.Development.json`、Aspire settings、`.env`、JWT secret、连接字符串、模型供应商密钥和前端调试 token 都应按环境替换。

## 贡献指南

提交前请运行相关构建、格式化和检查命令。保持变更聚焦，并在 PR 中说明变更摘要、影响模块、配置变化、迁移影响和验证结果。UI 变更建议附截图或录屏。

## 许可证

当前仓库未包含许可证文件。使用、分发或商业化前请先确认项目授权。
