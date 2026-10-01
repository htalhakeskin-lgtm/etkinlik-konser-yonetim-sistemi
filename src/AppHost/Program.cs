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

// The connection string carries the administrator's credentials; the Host uses them only to prepare
// the database, then every module connects with its own role (building-blocks §5).
IResourceBuilder<ProjectResource> host = builder
    .AddProject<Projects.FestOS_Host>("host")
    .WithReference(database)
    .WaitFor(database);

// One generated password per database role, kept in this project's user secrets (building-blocks
// BB-07). A module's role is added here together with the module; the Host fails at startup when a
// registered module has no password.
string[] databaseRoles = ["festos_migrator", "festos_audit", "festos_identity", "festos_inventory"];
foreach (string role in databaseRoles)
{
    IResourceBuilder<ParameterResource> password = builder.AddParameter(
        $"{role.Replace('_', '-')}-password",
        new GenerateParameterDefault { MinLength = 32, Special = false },
        secret: true,
        persist: true
    );
    host.WithEnvironment($"Database__Passwords__{role}", password);
}

// Vite dev server; it proxies /api and /hubs to the Host (src/web/vite.config.ts). Packages are
// installed once with "pnpm install" at the repository root, so Aspire does not install them.
builder.AddViteApp("web", "../web").WithPnpm(install: false).WithReference(host).WaitFor(host);

await builder.Build().RunAsync();
