namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>What creating and editing a production both send, so one set of checks covers both (riders §6).</summary>
public interface IProductionCommand
{
    /// <summary>The name, unique for the artist.</summary>
    string Name { get; }

    /// <summary>What the show is.</summary>
    string? Description { get; }
}
