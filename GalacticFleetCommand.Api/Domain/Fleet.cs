namespace GalacticFleetCommand.Api.Domain;

public enum FleetState
{
    Docked,
    Preparing,
    Ready,
    Deployed,
    FailedPreparation
}

public class Fleet : IVersionedEntity
{
    public required string Id { get; init; }
    public int Version { get; set; }

    public required string Name { get; set; }
    public int ShipCount { get; set; }
    public int FuelRequired { get; set; }

    public FleetState State { get; set; }
}