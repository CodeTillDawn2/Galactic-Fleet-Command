using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Domain.Exceptions;

namespace GalacticFleetCommand.Tests.Unit;

public class FleetLifecycleTests
{
    [Fact]
    public void NewFleet_StartsDocked()
    {
        var fleet = CreateFleet();

        Assert.Equal(FleetState.Docked, fleet.State);
    }

    [Fact]
    public void BeginPreparation_WhenDocked_TransitionsToPreparing()
    {
        var fleet = CreateFleet();

        fleet.BeginPreparation();

        Assert.Equal(FleetState.Preparing, fleet.State);
    }

    [Fact]
    public void MarkReady_WhenPreparing_TransitionsToReady()
    {
        var fleet = CreateFleet();
        fleet.BeginPreparation();

        fleet.MarkReady();

        Assert.Equal(FleetState.Ready, fleet.State);
    }

    [Fact]
    public void FailPreparation_WhenPreparing_TransitionsToFailedPreparation()
    {
        var fleet = CreateFleet();
        fleet.BeginPreparation();

        fleet.FailPreparation();

        Assert.Equal(FleetState.FailedPreparation, fleet.State);
    }

    [Fact]
    public void Deploy_WhenReady_TransitionsToDeployed()
    {
        var fleet = CreateFleet();
        fleet.BeginPreparation();
        fleet.MarkReady();

        fleet.Deploy();

        Assert.Equal(FleetState.Deployed, fleet.State);
    }

    [Fact]
    public void BeginPreparation_WhenPreparing_ThrowsInvalidFleetStateTransitionException()
    {
        var fleet = CreateFleet();
        fleet.BeginPreparation();

        var ex = Assert.Throws<InvalidFleetStateTransitionException>(fleet.BeginPreparation);

        Assert.Equal(FleetState.Preparing, ex.CurrentState);
        Assert.Equal(FleetState.Preparing, ex.AttemptedState);
        Assert.Equal(FleetState.Docked, ex.ExpectedCurrentState);
    }

    [Fact]
    public void MarkReady_WhenDocked_ThrowsInvalidFleetStateTransitionException()
    {
        var fleet = CreateFleet();

        var ex = Assert.Throws<InvalidFleetStateTransitionException>(fleet.MarkReady);

        Assert.Equal(FleetState.Docked, ex.CurrentState);
        Assert.Equal(FleetState.Ready, ex.AttemptedState);
        Assert.Equal(FleetState.Preparing, ex.ExpectedCurrentState);
    }

    [Fact]
    public void FailPreparation_WhenDocked_ThrowsInvalidFleetStateTransitionException()
    {
        var fleet = CreateFleet();

        var ex = Assert.Throws<InvalidFleetStateTransitionException>(fleet.FailPreparation);

        Assert.Equal(FleetState.Docked, ex.CurrentState);
        Assert.Equal(FleetState.FailedPreparation, ex.AttemptedState);
        Assert.Equal(FleetState.Preparing, ex.ExpectedCurrentState);
    }

    [Fact]
    public void Deploy_WhenDocked_ThrowsInvalidFleetStateTransitionException()
    {
        var fleet = CreateFleet();

        var ex = Assert.Throws<InvalidFleetStateTransitionException>(fleet.Deploy);

        Assert.Equal(FleetState.Docked, ex.CurrentState);
        Assert.Equal(FleetState.Deployed, ex.AttemptedState);
        Assert.Equal(FleetState.Ready, ex.ExpectedCurrentState);
    }

    [Fact]
    public void Deploy_WhenPreparing_ThrowsInvalidFleetStateTransitionException()
    {
        var fleet = CreateFleet();
        fleet.BeginPreparation();

        var ex = Assert.Throws<InvalidFleetStateTransitionException>(fleet.Deploy);

        Assert.Equal(FleetState.Preparing, ex.CurrentState);
        Assert.Equal(FleetState.Deployed, ex.AttemptedState);
        Assert.Equal(FleetState.Ready, ex.ExpectedCurrentState);
    }

    private static Fleet CreateFleet()
    {
        return new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Version = 1,
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        };
    }
}