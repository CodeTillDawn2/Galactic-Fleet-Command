namespace GalacticFleetCommand.Api.Application;

/// <summary>
/// In-memory queue used to pass accepted commands from the API layer to the background worker.
/// </summary>
public interface IBackgroundCommandQueue
{
    ValueTask QueueAsync(string commandId, CancellationToken cancellationToken = default);
    ValueTask<string> DequeueAsync(CancellationToken cancellationToken);
}