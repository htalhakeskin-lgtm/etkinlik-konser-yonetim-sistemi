namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>
/// Marks a text field, such as a password, that must reach the handler exactly as sent: it is neither
/// trimmed nor normalized (api §5.3).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class SensitiveAttribute : Attribute;
