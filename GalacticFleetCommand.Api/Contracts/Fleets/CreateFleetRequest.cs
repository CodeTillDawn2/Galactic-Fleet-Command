/// <summary>
/// Request to create a new fleet.
/// </summary>
public class CreateFleetRequest
{
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
}