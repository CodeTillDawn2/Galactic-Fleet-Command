using GalacticFleetCommand.Api.Domain;

namespace GalacticFleetCommand.Api.Contracts.Commands;

/// <summary>
/// Request used to submit a fleet command.
/// </summary>
public class CreateCommandRequest
{
    /// <summary>
    /// Type of command to submit.
    /// </summary>
    public CommandType Type { get; init; }

    /// <summary>
    /// Id of the fleet targeted by the command.
    /// </summary>
    public required string FleetId { get; init; }
}