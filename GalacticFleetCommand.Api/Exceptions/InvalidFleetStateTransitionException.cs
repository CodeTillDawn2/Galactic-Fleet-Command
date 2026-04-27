namespace GalacticFleetCommand.Api.Domain.Exceptions;

using GalacticFleetCommand.Api.Domain;

public class InvalidFleetStateTransitionException : Exception
{
    public InvalidFleetStateTransitionException(
        FleetState currentState,
        FleetState attemptedState,
        FleetState expectedCurrentState)
        : base($"Cannot transition fleet from {currentState} to {attemptedState}. Expected current state: {expectedCurrentState}.")
    {
        CurrentState = currentState;
        AttemptedState = attemptedState;
        ExpectedCurrentState = expectedCurrentState;
    }

    public FleetState CurrentState { get; }
    public FleetState AttemptedState { get; }
    public FleetState ExpectedCurrentState { get; }
}