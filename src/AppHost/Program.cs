// Local development: starts PostgreSQL, the API host and (later) the front end with one command
// and shows their logs, traces and metrics in the Aspire dashboard (ADR-0016).
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL 18, initialised with the builtin C.UTF-8 collation (docs/standards/database.md §3).
IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithImageTag("18")
    .WithEnvironment("POSTGRES_INITDB_ARGS", "--locale-provider=builtin --builtin-locale=C.UTF-8")
    .WithDataVolume();

IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase("festos");

builder.AddProject<Projects.FestOS_Host>("host").WithReference(database).WaitFor(database);

await builder.Build().RunAsync();
