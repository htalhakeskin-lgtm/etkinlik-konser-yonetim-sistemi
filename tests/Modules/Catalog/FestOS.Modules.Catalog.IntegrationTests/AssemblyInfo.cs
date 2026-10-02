using FestOS.Modules.Catalog.IntegrationTests;
using Xunit.Sdk;
using Xunit.v3;

[assembly: AssemblyFixture(typeof(CatalogFixture))]

// The tests share one database, so they run one after another (testing.md §6).
[assembly: Parallelization(Mode = ParallelMode.None)]
