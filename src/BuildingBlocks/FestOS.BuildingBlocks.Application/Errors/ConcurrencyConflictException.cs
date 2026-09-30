namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>
/// Someone else changed the aggregate after the user loaded it; the change is rejected, never retried
/// silently. The API answers 412 (database §11.1, api §9).
/// </summary>
public sealed class ConcurrencyConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);
