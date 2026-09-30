namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>
/// The idempotency key was already used by the same user for a different request; the API answers 422
/// (api §10). A retry sends the same request with the same key, so this is a client error.
/// </summary>
public sealed class IdempotencyKeyReusedException(string message) : Exception(message);
