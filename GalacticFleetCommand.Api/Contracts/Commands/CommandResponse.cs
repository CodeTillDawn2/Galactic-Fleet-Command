using GalacticFleetCommand.Api.Domain;

namespace GalacticFleetCommand.Api.Contracts.Commands;

/// <summary>
/// Response returned for command submission and command status lookup.
/// </summary>
public class CommandResponse
{
    /// <summary>
    /// Command id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Command type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Current command processing status.
    /// </summary>
    public CommandStatus Status { get; init; }

    /// <summary>
    /// Id of the fleet targeted by the command.
    /// </summary>
    public required string FleetId { get; init; }
}