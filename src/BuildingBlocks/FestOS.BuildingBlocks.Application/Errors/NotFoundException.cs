namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>The requested record does not exist or is not visible to the user; the API answers 404.</summary>
public sealed class NotFoundException : Exception
{
    /// <summary>Creates the exception.</summary>
    /// <param name="resource">The kind of record, in English, e.g. <c>Event</c>.</param>
    /// <param name="resourceId">The identifier that was looked up.</param>
    public NotFoundException(string resource, Guid resourceId)
        : base($"{resource} {resourceId} was not found.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        Resource = resource;
        ResourceId = resourceId;
    }

    /// <summary>The kind of record.</summary>
    public string Resource { get; }

    /// <summary>The identifier that was looked up.</summary>
    public Guid ResourceId { get; }
}
