using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgresdb = builder.AddPostgres("postgres")
    .WithDataVolume("postgres-aicopilot")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050))
    .AddDatabase("ai-copilot");

var migration = builder.AddProject<Zilor_AICopilot_MigrationWorkApp>("aicopilot-migration")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Zilor_AICopilot_HttpApi>("aicopilot-httpapi")
    .WithUrl("swagger")
    .WaitFor(postgresdb)
    .WithReference(postgresdb)
    .WithReference(migration)
    .WaitForCompletion(migration);

builder.Build().Run();