// Local development: starts PostgreSQL, the API host and the front end with one command
// and shows their logs, traces and metrics in the Aspire dashboard (ADR-0016).
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL 18, initialised with the builtin C.UTF-8 collation (docs/standards/database.md §3).
IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithImageTag("18")
    .WithEnvironment("POSTGRES_INITDB_ARGS", "--locale-provider=builtin --builtin-locale=C.UTF-8")
    .WithDataVolume();

IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase("festos");

IResourceBuilder<ProjectResource> host = builder
    .AddProject<Projects.FestOS_Host>("host")
    .WithReference(database)
    .WaitFor(database);

// Vite dev server; it proxies /api and /hubs to the Host (src/web/vite.config.ts). Packages are
// installed once with "pnpm install" at the repository root, so Aspire does not install them.
builder.AddViteApp("web", "../web").WithPnpm(install: false).WithReference(host).WaitFor(host);

await builder.Build().RunAsync();
