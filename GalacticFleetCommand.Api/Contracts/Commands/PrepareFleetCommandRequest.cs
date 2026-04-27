namespace GalacticFleetCommand.Api.Contracts.Commands;

/// <summary>
/// Request used to submit a prepare fleet command.
/// </summary>
public class CreatePrepareFleetCommandRequest
{
    /// <summary>
    /// Id of the fleet to prepare.
    /// </summary>
    public required string FleetId { get; init; }
}