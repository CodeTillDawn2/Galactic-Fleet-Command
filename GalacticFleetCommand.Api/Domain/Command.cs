namespace GalacticFleetCommand.Api.Domain;

public enum CommandStatus
{
    Queued,
    Processing,
    Succeeded,
    Failed
}

public class Command : IVersionedEntity
{
    public required string Id { get; init; }
    public int Version { get; set; }

    public CommandType Type { get; set; }
    public CommandStatus Status { get; set; }

    public Dictionary<string, object?> Payload { get; set; } = [];

    public string? FailureReason { get; set; }
}