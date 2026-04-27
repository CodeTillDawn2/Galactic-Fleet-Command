namespace GalacticFleetCommand.Api.Contracts.Fleets;

/// <summary>
/// Request to update an existing fleet's editable properties.
/// </summary>
public class UpdateFleetRequest
{
    /// <summary>
    /// Updated display name of the fleet.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Updated number of ships in the fleet.
    /// </summary>
    public int? ShipCount { get; init; }

    /// <summary>
    /// Updated fuel required to prepare the fleet.
    /// </summary>
    public int? FuelRequired { get; init; }
}