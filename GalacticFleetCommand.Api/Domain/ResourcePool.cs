namespace GalacticFleetCommand.Api.Domain;

public enum ResourceType
{
    Fuel
}

public class ResourcePool : IVersionedEntity
{
    public required string Id { get; init; }
    public int Version { get; set; }

    public ResourceType ResourceType { get; set; }

    public int Total { get; set; }
    public int Reserved { get; set; }
}

public class ResourceAvailability
{
    public ResourceType ResourceType { get; init; }
    public int Total { get; init; }
    public int Reserved { get; init; }
    public int Available => Total - Reserved;
}