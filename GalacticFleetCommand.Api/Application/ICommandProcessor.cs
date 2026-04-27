namespace GalacticFleetCommand.Api.Application;

/// <summary>
/// Processes commands that have been accepted and queued.
/// </summary>
public interface ICommandProcessor
{
    Task ProcessAsync(string commandId, CancellationToken cancellationToken);
}