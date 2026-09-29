namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>One failed validation rule on one field of a request.</summary>
/// <param name="PropertyPath">The field, as FluentValidation names it, e.g. <c>Units[3].SerialNumber</c>.</param>
/// <param name="Code">The FluentValidation error code, e.g. <c>NotEmptyValidator</c>; the API maps it (api §8.2).</param>
/// <param name="Parameters">Values the message needs, e.g. the maximum length; never the submitted value.</param>
public sealed record ValidationError(string PropertyPath, string Code, IReadOnlyDictionary<string, object?> Parameters);
