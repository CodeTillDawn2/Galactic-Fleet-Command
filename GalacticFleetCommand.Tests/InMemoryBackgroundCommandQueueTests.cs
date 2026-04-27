using GalacticFleetCommand.Api.Application;

namespace GalacticFleetCommand.Tests.Application;

public class InMemoryBackgroundCommandQueueTests
{
    [Fact]
    public async Task DequeueAsync_ReturnsQueuedCommandId()
    {
        var queue = new InMemoryBackgroundCommandQueue();
        var commandId = Guid.NewGuid().ToString();

        await queue.QueueAsync(commandId);

        var dequeued = await queue.DequeueAsync(CancellationToken.None);

        Assert.Equal(commandId, dequeued);
    }

    [Fact]
    public async Task DequeueAsync_ReturnsCommandsInOrder()
    {
        var queue = new InMemoryBackgroundCommandQueue();

        await queue.QueueAsync("first");
        await queue.QueueAsync("second");

        var first = await queue.DequeueAsync(CancellationToken.None);
        var second = await queue.DequeueAsync(CancellationToken.None);

        Assert.Equal("first", first);
        Assert.Equal("second", second);
    }

    [Fact]
    public async Task QueueAsync_WithMissingCommandId_ThrowsArgumentException()
    {
        var queue = new InMemoryBackgroundCommandQueue();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            queue.QueueAsync("").AsTask());
    }
}