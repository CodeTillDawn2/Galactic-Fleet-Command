# Galactic-Fleet-Command
Take home test for Arrowhead

## Design Notes

C# was chosen for this implementation due to its strong support for web APIs, background processing, and in-memory data structures, which align well with the requirements of the assignment.

The initial C# structure mirrors the provided TypeScript starter rather than introducing additional architectural layers up front.

The fleet model was extended with `shipCount` and `fuelRequired` because those fields are required by the assignment but were not present in the starter boilerplate.

Initially, Minimal APIs were used for their similarity to the provided starter, but the HTTP layer was refactored to controllers to provide a more conventional and scalable structure.

Although the current assignment has a small API surface, the domain suggests room for additional commands, fleet operations, and resource workflows. Controllers help keep the HTTP layer organized as that surface expands.

API contracts are separate from domain models so the HTTP surface can evolve independently from internal state and behavior. Clients can create or update fleet properties, while lifecycle state changes are handled through commands.

Init accessors are used instead of set to ensure contract objects are only assigned during initialization, reinforcing immutability for request and response models.

Fleet controller actions are kept thin and delegate business behavior to `FleetService`. This keeps HTTP concerns separate from fleet creation, validation, and update rules, making the behavior easier to test and less dependent on the web framework.

Controllers are responsible for request/response mapping, while the service layer operates on domain and contract models without knowledge of HTTP semantics.

Fleet lifecycle rules are enforced within the domain model so invalid state transitions cannot occur. State is not set directly; lifecycle changes are exposed through intention-revealing methods such as `BeginPreparation`, `MarkReady`, `FailPreparation`, and `Deploy`.

Invalid transitions use a domain-specific exception that exposes the current state, attempted state, and expected current state. This keeps transition failures structured for tests and later command processing instead of relying on exception message parsing.

Introduced a queue/worker boundary before implementing command behavior to enforce separation between synchronous request handling and asynchronous execution.

Used a `Channel`-backed in-memory queue instead of a manually synchronized collection (`Queue` + `SemaphoreSlim`), as the problem is producer/consumer coordination rather than mutual exclusion. This reduces complexity while correctly supporting multiple producers and a single consumer.

Introduced a command processor abstraction to prevent command-specific logic from accumulating in the background worker as additional command types are added.

Command execution is handled in the processor rather than the service layer to align with the asynchronous execution model.

Command outcomes are represented in state rather than exceptions, allowing failures to be observed without interrupting processing.

Command status is treated as execution metadata and managed by the processor rather than the domain model.

Resource reservation is performed within a single repository update to ensure availability checks and reservation occur atomically.

Insufficient resources are treated as an expected outcome rather than an error, allowing command execution to complete with a failed preparation state.

Removed duplicate PrepareFleetCommand tests to keep coverage focused in CommandProcessor tests, avoiding overlapping test suites.

Extended command submission through the existing `/commands` endpoint rather than adding command-specific routes, keeping the API centered on command submission.

Replaced command type strings with a `CommandType` enum so command creation, dispatch, and tests share one type-safe representation.

Reused the existing command processing pipeline for deployment, avoiding a separate execution path for the new command type.

## Implementation Approach

### 1. Convert starter boilerplate to C# and extend the fleet model

Translate the provided TypeScript starter into C#, preserving the same core structure and behavior.

Extend the starter fleet model with `shipCount` and `fuelRequired`.

### 2. Refactor HTTP layer to controllers and add health endpoint test

Introduce controllers to define the API surface and handle request/response concerns.

This separates HTTP handling from application startup, keeping `Program.cs` focused on composition while controllers manage routing and input/output concerns.

Keep `Program.cs` focused on startup, dependency registration, and middleware configuration.

Move existing "health" endpoint into a dedicated controller for consistency.

Implement health endpoint test to verify it returns the expected response.

### 3. Add API contracts

Define request and response contracts for controller actions instead of exposing domain entities directly.

### 4. Add fleet controller

Implement:

POST `/fleets`  
GET `/fleets/{id}`  
PATCH `/fleets/{id}`

Include validation, expected error responses, logging, and tests.

### 5. Enforce fleet lifecycle rules

Centralize valid fleet transitions in the domain model:

Docked -> Preparing  
Preparing -> Ready  
Preparing -> FailedPreparation  
Ready -> Deployed

Handle invalid transitions explicitly with a domain-specific exception so future command handlers can map lifecycle failures to clear command failure reasons or API responses.

### 6. Add command controller

Implement:

POST `/commands`  
GET `/commands/{id}`

Include validation, expected error responses, logging, and tests.

### 7. Introduce asynchronous processing boundary

Add an in-memory command queue and a single background worker.

Treat API submission as the producer and the worker as the consumer, forming a simple event-driven processing model.

### 8. Implement PrepareFleetCommand

Implement the required workflow:

Docked -> Preparing  
Preparing -> Ready on successful resource reservation  
Preparing -> FailedPreparation on failed reservation

Record command failure reasons when processing fails.

### 9. Add resource reservation behavior

Use the shared fuel resource pool to reserve the fuel required by a fleet before it can become Ready.

Handle insufficient fuel as an expected outcome.

Ensure the availability check and reservation update happen atomically so fuel cannot be over-allocated.

### 10. Add DeployFleetCommand

Implement a deployment command that transitions fleets from Ready to Deployed.

### 11. Add fleet transition history

Record significant fleet state transitions so the lifecycle can be inspected after commands are processed.

### 12. Add tests throughout implementation

Cover:

Valid and invalid fleet state transitions  
Resource reservation under concurrent conditions  
One end-to-end flow from API request to command processing to fleet state change  
DeployFleetCommand success and failure cases  
Fleet transition history recording


## Future Improvements

Expose command processing metrics such as queued count, success/failure counts, and processing duration through a lightweight metrics endpoint or OpenTelemetry.