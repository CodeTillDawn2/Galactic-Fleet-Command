using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Contracts.Commands;
using GalacticFleetCommand.Api.Controllers;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace GalacticFleetCommand.Tests.Controllers;

public class CommandControllerTests
{
    private readonly InMemoryCommandRepository commandRepository = new();
    private readonly InMemoryFleetRepository fleetRepository = new();

    [Fact]
    public void Create_WithValidRequest_ReturnsCreatedCommand()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var controller = CreateController();

        var result = controller.Create(new CreatePrepareFleetCommandRequest
        {
            FleetId = fleet.Id
        });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<CommandResponse>(createdResult.Value);

        Assert.Equal(nameof(CommandController.Get), createdResult.ActionName);
        Assert.Equal("PrepareFleetCommand", response.Type);
        Assert.Equal(CommandStatus.Queued, response.Status);
        Assert.Equal(fleet.Id, response.FleetId);
    }

    [Fact]
    public void Create_WithMissingFleetId_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = controller.Create(new CreatePrepareFleetCommandRequest
        {
            FleetId = ""
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Create_WithUnknownFleet_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = controller.Create(new CreatePrepareFleetCommandRequest
        {
            FleetId = "missing-fleet"
        });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Get_WithExistingCommand_ReturnsOk()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var controller = CreateController();

        var createResult = Assert.IsType<CreatedAtActionResult>(
            controller.Create(new CreatePrepareFleetCommandRequest
            {
                FleetId = fleet.Id
            }));

        var createdCommand = Assert.IsType<CommandResponse>(createResult.Value);

        var getResult = controller.Get(createdCommand.Id);

        var okResult = Assert.IsType<OkObjectResult>(getResult);
        var response = Assert.IsType<CommandResponse>(okResult.Value);

        Assert.Equal(createdCommand.Id, response.Id);
        Assert.Equal(fleet.Id, response.FleetId);
    }

    [Fact]
    public void Get_WithUnknownCommand_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = controller.Get("missing-command");

        Assert.IsType<NotFoundResult>(result);
    }

    private CommandController CreateController()
    {
        var service = new CommandService(commandRepository, fleetRepository);

        return new CommandController(
            service,
            NullLogger<CommandController>.Instance);
    }
}