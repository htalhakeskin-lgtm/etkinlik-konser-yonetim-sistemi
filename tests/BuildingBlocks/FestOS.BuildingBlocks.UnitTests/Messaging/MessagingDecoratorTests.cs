using System.Diagnostics;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Sample.Application;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace FestOS.BuildingBlocks.UnitTests.Messaging;

// Tests in one class run one after another, so the listener sees only this test's spans.
public sealed class MessagingDecoratorTests : IDisposable
{
    private readonly List<Activity> _activities = [];
    private readonly ActivityListener _listener;
    private readonly SampleHandlerProbe _probe = new();
    private readonly ServiceProvider _services;

    public MessagingDecoratorTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => string.Equals(source.Name, "FestOS.BuildingBlocks", StringComparison.Ordinal),
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = _activities.Add,
        };
        ActivitySource.AddActivityListener(_listener);

        _services = CreateServices()
            .AddSingleton(_probe)
            .DecorateHandlers()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReachesTheHandler()
    {
        Guid sampleId = await SendAsync(new PlaceSampleCommand("Stage"));

        sampleId.ShouldNotBe(Guid.Empty);
        _probe.Calls.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_ThrowsWithoutReachingTheHandler()
    {
        ValidationFailedException exception = await Should.ThrowAsync<ValidationFailedException>(() =>
            SendAsync(new PlaceSampleCommand(""))
        );

        ValidationError error = exception.Errors.ShouldHaveSingleItem();
        error.PropertyPath.ShouldBe("Name");
        error.Code.ShouldBe("NotEmptyValidator");
        _probe.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_WithTooLongValue_ReportsTheLimitButNotTheValue()
    {
        ValidationFailedException exception = await Should.ThrowAsync<ValidationFailedException>(() =>
            SendAsync(new PlaceSampleCommand("Main stage left"))
        );

        IReadOnlyDictionary<string, object?> parameters = exception.Errors.ShouldHaveSingleItem().Parameters;
        parameters["MaxLength"].ShouldBe(10);
        parameters.ShouldNotContainKey("PropertyValue");
    }

    [Fact]
    public async Task HandleAsync_ForACommand_TracesASpanNamedAfterItWithTheModule()
    {
        await SendAsync(new PlaceSampleCommand("Stage"));

        Activity activity = _activities.ShouldHaveSingleItem();
        activity.DisplayName.ShouldBe("PlaceSample");
        activity.GetTagItem("festos.module").ShouldBe("Sample");
        activity.Status.ShouldBe(ActivityStatusCode.Unset);
    }

    [Fact]
    public async Task HandleAsync_WhenARuleIsViolated_TagsTheRuleCodeWithoutMarkingAnError()
    {
        _probe.Failure = new BusinessRuleViolationException("SAMPLE-001", "Sample rule violated.");

        await Should.ThrowAsync<BusinessRuleViolationException>(() => SendAsync(new PlaceSampleCommand("Stage")));

        Activity activity = _activities.ShouldHaveSingleItem();
        activity.GetTagItem("festos.error.code").ShouldBe("SAMPLE-001");
        activity.Status.ShouldBe(ActivityStatusCode.Unset);
    }

    [Fact]
    public async Task HandleAsync_WhenTheHandlerFailsUnexpectedly_MarksTheSpanAsError()
    {
        _probe.Failure = new InvalidOperationException("Sample failure.");

        await Should.ThrowAsync<InvalidOperationException>(() => SendAsync(new PlaceSampleCommand("Stage")));

        _activities.ShouldHaveSingleItem().Status.ShouldBe(ActivityStatusCode.Error);
    }

    [Fact]
    public async Task HandleAsync_ForAQuery_ValidatesAndTraces()
    {
        await Should.ThrowAsync<ValidationFailedException>(() => AskAsync(new GetSampleQuery(Guid.Empty)));

        string result = await AskAsync(new GetSampleQuery(Guid.CreateVersion7()));

        result.ShouldBe("sample");
        _activities.Select(activity => activity.DisplayName).ShouldBe(["GetSample", "GetSample"]);
    }

    [Fact]
    public void AddHandlersFrom_ForValidators_RegistersOnlyTheValidatorInterface()
    {
        IServiceCollection services = CreateServices();

        services.ShouldContain(descriptor => descriptor.ServiceType == typeof(IValidator<PlaceSampleCommand>));
        services.ShouldNotContain(descriptor => descriptor.ServiceType == typeof(IEnumerable<IValidationRule>));
    }

    public void Dispose()
    {
        _listener.Dispose();
        _services.Dispose();
    }

    private static IServiceCollection CreateServices() =>
        new ServiceCollection()
            .AddSingleton<ICurrentUser, FakeCurrentUser>()
            .AddSingleton<TimeProvider>(new FakeTimeProvider())
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddHandlersFrom(typeof(PlaceSampleHandler).Assembly);

    private async Task<Guid> SendAsync(PlaceSampleCommand command)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ICommandHandler<PlaceSampleCommand, Guid> handler = scope.ServiceProvider.GetRequiredService<
            ICommandHandler<PlaceSampleCommand, Guid>
        >();
        return await handler.HandleAsync(command, TestContext.Current.CancellationToken);
    }

    private async Task<string> AskAsync(GetSampleQuery query)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IQueryHandler<GetSampleQuery, string> handler = scope.ServiceProvider.GetRequiredService<
            IQueryHandler<GetSampleQuery, string>
        >();
        return await handler.HandleAsync(query, TestContext.Current.CancellationToken);
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public Guid UserId => SystemUser.Id;

        public string DisplayName => SystemUser.Name;
    }
}
