namespace GalacticFleetCommand.Api.Contracts.Fleets;

/// <summary>
/// Represents a fleet returned from the API.
/// </summary>
public class FleetResponse
{
    /// <summary>
    /// Unique identifier of the fleet.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the fleet.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Number of ships in the fleet.
    /// </summary>
    public int ShipCount { get; init; }

    /// <summary>
    /// Fuel required to prepare the fleet.
    /// </summary>
    public int FuelRequired { get; init; }

    /// <summary>
    /// Current lifecycle state of the fleet.
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// Lifecycle state transitions recorded for the fleet.
    /// </summary>
    public IReadOnlyList<FleetTransitionResponse> Transitions { get; init; } = [];
}