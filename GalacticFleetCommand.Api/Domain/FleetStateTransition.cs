namespace GalacticFleetCommand.Api.Domain;

/// <summary>
/// Records a fleet lifecycle state transition.
/// </summary>
public class FleetStateTransition
{
    /// <summary>
    /// State before the transition.
    /// </summary>
    public FleetState From { get; init; }

    /// <summary>
    /// State after the transition.
    /// </summary>
    public FleetState To { get; init; }

    /// <summary>
    /// UTC timestamp when the transition occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; init; }
}