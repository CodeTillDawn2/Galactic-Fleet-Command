using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Contracts.Commands;
using GalacticFleetCommand.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace GalacticFleetCommand.Api.Controllers;

[ApiController]
[Route("commands")]
[Produces("application/json")]
public class CommandController : ControllerBase
{
    private readonly CommandService commandService;
    private readonly ILogger<CommandController> logger;

    public CommandController(CommandService commandService, ILogger<CommandController> logger)
    {
        this.commandService = commandService;
        this.logger = logger;
    }

    /// <summary>
    /// Submits a prepare fleet command.
    /// </summary>
    /// <param name="request">Prepare fleet command request.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The queued command.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CommandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CreatePrepareFleetCommandRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = await commandService.CreatePrepareFleetCommandAsync(request, cancellationToken);

            logger.LogInformation("Created command {CommandId} for fleet {FleetId}", command.Id, command.FleetId);

            return CreatedAtAction(nameof(Get), new { id = command.Id }, command);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Retrieves command status by id.
    /// </summary>
    /// <param name="id">Command id.</param>
    /// <returns>The requested command.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(string id)
    {
        try
        {
            return Ok(commandService.Get(id));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}