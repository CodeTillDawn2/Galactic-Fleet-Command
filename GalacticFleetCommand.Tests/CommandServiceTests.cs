using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Contracts.Commands;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Tests.Application;

public class CommandServiceTests
{
    private readonly InMemoryCommandRepository commandRepository = new();
    private readonly InMemoryFleetRepository fleetRepository = new();

    [Fact]
    public void CreatePrepareFleetCommand_WithValidFleet_CreatesQueuedCommand()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var service = new CommandService(commandRepository, fleetRepository);

        var response = service.CreatePrepareFleetCommand(new CreatePrepareFleetCommandRequest
        {
            FleetId = fleet.Id
        });

        Assert.False(string.IsNullOrWhiteSpace(response.Id));
        Assert.Equal("PrepareFleetCommand", response.Type);
        Assert.Equal(CommandStatus.Queued, response.Status);
        Assert.Equal(fleet.Id, response.FleetId);
    }

    [Fact]
    public void CreatePrepareFleetCommand_WithMissingFleetId_ThrowsArgumentException()
    {
        var service = new CommandService(commandRepository, fleetRepository);

        var exception = Assert.Throws<ArgumentException>(() =>
            service.CreatePrepareFleetCommand(new CreatePrepareFleetCommandRequest
            {
                FleetId = ""
            }));

        Assert.Equal("Fleet id is required", exception.Message);
    }

    [Fact]
    public void CreatePrepareFleetCommand_WithUnknownFleet_ThrowsNotFoundException()
    {
        var service = new CommandService(commandRepository, fleetRepository);

        Assert.Throws<NotFoundException>(() =>
            service.CreatePrepareFleetCommand(new CreatePrepareFleetCommandRequest
            {
                FleetId = "missing-fleet"
            }));
    }

    [Fact]
    public void Get_WithExistingCommand_ReturnsCommand()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var service = new CommandService(commandRepository, fleetRepository);

        var created = service.CreatePrepareFleetCommand(new CreatePrepareFleetCommandRequest
        {
            FleetId = fleet.Id
        });

        var fetched = service.Get(created.Id);

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Type, fetched.Type);
        Assert.Equal(created.Status, fetched.Status);
        Assert.Equal(created.FleetId, fetched.FleetId);
    }

    [Fact]
    public void Get_WithUnknownCommand_ThrowsNotFoundException()
    {
        var service = new CommandService(commandRepository, fleetRepository);

        Assert.Throws<NotFoundException>(() => service.Get("missing-command"));
    }
}