namespace GalacticFleetCommand.Api.Contracts.Fleets;

/// <summary>
/// Represents a fleet lifecycle state transition returned from the API.
/// </summary>
public class FleetTransitionResponse
{
    /// <summary>
    /// State before the transition.
    /// </summary>
    public required string From { get; init; }

    /// <summary>
    /// State after the transition.
    /// </summary>
    public required string To { get; init; }

    /// <summary>
    /// UTC timestamp when the transition occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; init; }
}