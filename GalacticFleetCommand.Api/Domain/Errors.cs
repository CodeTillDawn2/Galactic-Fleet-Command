namespace GalacticFleetCommand.Api.Domain;

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string entityId, int expectedVersion, int actualVersion)
        : base($"Concurrency conflict: entity {entityId} expected version {expectedVersion} but was {actualVersion}")
    {
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public string EntityId { get; }
    public int ExpectedVersion { get; }
    public int ActualVersion { get; }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entityId, string? entityType = null)
        : base(entityType is null ? $"Entity not found: {entityId}" : $"{entityType} not found: {entityId}")
    {
        EntityId = entityId;
        EntityType = entityType;
    }

    public string EntityId { get; }
    public string? EntityType { get; }
}

public class DuplicateIdException : Exception
{
    public DuplicateIdException(string entityId)
        : base($"Entity already exists: {entityId}")
    {
        EntityId = entityId;
    }

    public string EntityId { get; }
}