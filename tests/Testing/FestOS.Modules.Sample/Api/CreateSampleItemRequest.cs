namespace FestOS.Modules.Sample.Api;

/// <summary>The body of <c>POST /api/v1/sample-items</c>.</summary>
internal sealed record CreateSampleItemRequest(string Name, decimal UnitPrice);
