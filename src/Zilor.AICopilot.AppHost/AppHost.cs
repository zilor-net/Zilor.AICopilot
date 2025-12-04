using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgresdb = builder.AddPostgres("postgres")
    .WithDataVolume("postgres-aicopilot")
    .WithPgWeb(pgAdmin => pgAdmin.WithHostPort(5050))
    .AddDatabase("ai-copilot");

var rabbitmq = builder.AddRabbitMQ("eventbus")
    .WithManagementPlugin(5672)
    .WithLifetime(ContainerLifetime.Persistent);

var qdrant = builder.AddQdrant("qdrant")
    .WithDataVolume("qdrant-data"); // 持久化数据

var migration = builder.AddProject<Zilor_AICopilot_MigrationWorkApp>("aicopilot-migration")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Zilor_AICopilot_HttpApi>("aicopilot-httpapi")
    .WithUrl("swagger")
    .WaitFor(postgresdb)
    .WaitFor(rabbitmq)
    .WaitFor(qdrant)
    .WithReference(postgresdb)
    .WithReference(rabbitmq)
    .WithReference(qdrant)
    .WithReference(migration)
    .WaitForCompletion(migration);

builder.AddProject<Zilor_AICopilot_RagWorker>("rag-worker")
    .WithReference(postgresdb) // 注入数据库连接
    .WithReference(rabbitmq) // 注入 RabbitMQ 连接
    .WaitFor(qdrant)
    .WaitFor(postgresdb) // 等待数据库启动
    .WaitFor(rabbitmq) // 等待 MQ 启动
    .WithReference(qdrant);

builder.Build().Run();