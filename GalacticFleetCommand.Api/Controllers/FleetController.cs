using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Contracts.Fleets;
using GalacticFleetCommand.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace GalacticFleetCommand.Api.Controllers;

[ApiController]
[Route("fleets")]
[Produces("application/json")]
public class FleetController : ControllerBase
{
    private readonly FleetService fleetService;
    private readonly ILogger<FleetController> logger;

    public FleetController(FleetService fleetService, ILogger<FleetController> logger)
    {
        this.fleetService = fleetService;
        this.logger = logger;
    }

    /// <summary>
    /// Creates a new fleet.
    /// </summary>
    /// <param name="request">Fleet creation request.</param>
    /// <returns>The newly created fleet.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(FleetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create(CreateFleetRequest request)
    {
        try
        {
            var fleet = fleetService.Create(request);

            logger.LogInformation("Created fleet {FleetId}", fleet.Id);

            return CreatedAtAction(nameof(Get), new { id = fleet.Id }, fleet);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a fleet by id.
    /// </summary>
    /// <param name="id">Fleet id.</param>
    /// <returns>The requested fleet.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FleetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(string id)
    {
        try
        {
            return Ok(fleetService.Get(id));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Updates editable fleet properties.
    /// </summary>
    /// <param name="id">Fleet id.</param>
    /// <param name="request">Fleet update request.</param>
    /// <returns>The updated fleet.</returns>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(FleetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Update(string id, UpdateFleetRequest request)
    {
        try
        {
            var fleet = fleetService.Update(id, request);

            logger.LogInformation("Updated fleet {FleetId}", fleet.Id);

            return Ok(fleet);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}