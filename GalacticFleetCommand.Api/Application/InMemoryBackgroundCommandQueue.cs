using System.Threading.Channels;

namespace GalacticFleetCommand.Api.Application;

public class InMemoryBackgroundCommandQueue : IBackgroundCommandQueue
{
    private readonly Channel<string> queue = Channel.CreateUnbounded<string>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask QueueAsync(string commandId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(commandId))
            throw new ArgumentException("Command id is required", nameof(commandId));

        return queue.Writer.WriteAsync(commandId, cancellationToken);
    }

    public ValueTask<string> DequeueAsync(CancellationToken cancellationToken)
    {
        return queue.Reader.ReadAsync(cancellationToken);
    }
}