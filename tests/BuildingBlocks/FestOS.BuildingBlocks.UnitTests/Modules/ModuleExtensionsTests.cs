using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.Modules.Sample.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.BuildingBlocks.UnitTests.Modules;

public sealed class ModuleExtensionsTests
{
    [Fact]
    public void AddModules_WithSeveralModules_RegistersThemInOrder()
    {
        List<string> registered = [];
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        builder.AddModules(
            new SampleModuleDefinition("Sample", "sample", registered),
            new SampleModuleDefinition("Other", "other", registered)
        );

        registered.ShouldBe(["Sample", "Other"]);
        using ServiceProvider services = builder.Services.BuildServiceProvider();
        services
            .GetRequiredService<ModuleCatalog>()
            .Modules.Select(module => module.Name)
            .ShouldBe(["Sample", "Other"]);
    }

    [Theory]
    [InlineData("Sample", "other")]
    [InlineData("Other", "sample")]
    public void AddModules_WithRepeatedNameOrSchema_Throws(string name, string schema)
    {
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        Should.Throw<InvalidOperationException>(() =>
            builder.AddModules(new SampleModuleDefinition("Sample", "sample"), new SampleModuleDefinition(name, schema))
        );
    }

    [Fact]
    public void AddModules_WithSchemaThatCannotBeARoleName_Throws()
    {
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        Should.Throw<ArgumentException>(() => builder.AddModules(new SampleModuleDefinition("Sample", "Sample")));
    }

    [Fact]
    public void AddModules_ForModuleHandlers_WrapsEachHandlerOnce()
    {
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        builder.AddModules(new SampleModuleDefinition("Sample", "sample"));

        List<ServiceDescriptor> registrations =
        [
            .. builder.Services.Where(descriptor =>
                descriptor.ServiceType == typeof(ICommandHandler<PlaceSampleCommand, Guid>)
            ),
        ];

        // Scrutor keeps each wrapped layer as a keyed registration: the handler, the unit of work and
        // the validation decorator. Wrapping twice would leave six.
        registrations.Count(descriptor => !descriptor.IsKeyedService).ShouldBe(1);
        registrations.Count(descriptor => descriptor.IsKeyedService).ShouldBe(3);
    }

    [Fact]
    [Trait("ArchitectureRule", "AT-10")]
    public void AddModules_ForIntegrationEventListeners_WrapsEachInTheInboxDecorator()
    {
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        builder.AddModules(new SampleModuleDefinition("Sample", "sample"));

        List<ServiceDescriptor> registrations =
        [
            .. builder.Services.Where(descriptor =>
                descriptor.ServiceType == typeof(IIntegrationEventHandler<SamplePlacedIntegrationEvent>)
            ),
        ];

        // The unkeyed registration is the inbox decorator; the listener itself is kept as a keyed one.
        registrations.Count(descriptor => !descriptor.IsKeyedService).ShouldBe(1);
        registrations.Count(descriptor => descriptor.IsKeyedService).ShouldBe(1);
    }

    [Fact]
    public async Task MapModules_ForAModule_MapsItsEndpointsUnderTheApiPrefixWithItsTag()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.AddModules(new SampleModuleDefinition("Sample", "sample"));
        await using WebApplication app = builder.Build();

        app.MapModules();

        RouteEndpoint endpoint = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ShouldHaveSingleItem();
        endpoint.RoutePattern.RawText.ShouldBe("/api/v1/samples");
        endpoint.Metadata.GetMetadata<ITagsMetadata>().ShouldNotBeNull().Tags.ShouldBe(["Sample"]);
    }

    private sealed class SampleModuleDefinition(string name, string schema, List<string>? registered = null)
        : IModuleDefinition
    {
        public string Name => name;

        public string Schema => schema;

        public IReadOnlyCollection<string> Permissions => [];

        public void RegisterServices(IHostApplicationBuilder builder)
        {
            registered?.Add(name);
            builder.Services.AddHandlersFrom(typeof(PlaceSampleHandler).Assembly);
        }

        public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapGet("/samples", () => Results.Ok());
    }
}
