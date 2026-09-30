namespace FestOS.BuildingBlocks.Domain.Entities;

/// <summary>
/// Keeps a field, or a whole entity, out of the change history, e.g. a password hash or a session key
/// (database §14.2).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class NotAuditedAttribute : Attribute;
