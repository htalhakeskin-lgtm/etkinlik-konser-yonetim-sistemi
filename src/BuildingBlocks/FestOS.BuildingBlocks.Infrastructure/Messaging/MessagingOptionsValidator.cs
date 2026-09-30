using Microsoft.Extensions.Options;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Validates <see cref="MessagingOptions"/> with generated code (configuration §4).</summary>
[OptionsValidator]
internal sealed partial class MessagingOptionsValidator : IValidateOptions<MessagingOptions>;
