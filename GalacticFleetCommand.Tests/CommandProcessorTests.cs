using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace GalacticFleetCommand.Tests.Application;

public class CommandProcessorTests
{
    [Fact]
    public async Task ProcessAsync_WithExistingCommand_Completes()
    {
        var repository = new InMemoryCommandRepository();

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = "PrepareFleetCommand",
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                ["fleetId"] = Guid.NewGuid().ToString()
            }
        };

        repository.Create(command);

        var processor = new CommandProcessor(
            repository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);
    }

    [Fact]
    public async Task ProcessAsync_WithUnknownCommand_ThrowsNotFoundException()
    {
        var processor = new CommandProcessor(
            new InMemoryCommandRepository(),
            NullLogger<CommandProcessor>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            processor.ProcessAsync("missing-command", CancellationToken.None));
    }
}