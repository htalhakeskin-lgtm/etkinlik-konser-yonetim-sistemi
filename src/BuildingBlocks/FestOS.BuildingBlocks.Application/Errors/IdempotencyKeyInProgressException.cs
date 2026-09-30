namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>
/// An earlier request with the same idempotency key is still running and did not finish within the lock
/// timeout; the API answers 409 and the client retries later with the same key (api §10).
/// </summary>
public sealed class IdempotencyKeyInProgressException(string message, Exception? innerException = null)
    : Exception(message, innerException);
