using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgresdb = builder.AddPostgres("postgres")
    .WithDataVolume("postgres-aicopilot")
    .WithPgWeb(pgWeb => pgWeb.WithHostPort(5050))
    .AddDatabase("ai-copilot");

var migration = builder.AddProject<Zilor_AICopilot_MigrationWorkApp>("aicopilot-migration")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Zilor_AICopilot_HttpApi>("aicopilot-httpapi")
    .WaitFor(postgresdb)
    .WithReference(postgresdb)
    .WithReference(migration)
    .WaitForCompletion(migration);

builder.Build().Run();