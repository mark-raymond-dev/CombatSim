using CombatSim.Core.Features.Simulator.Models;
using CombatSim.Core.Features.Simulator.Services;
using Microsoft.AspNetCore.Mvc;
using CombatSim.Api.Models;
using CombatSim.Api.Mapping;

namespace CombatSim.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulatorController : ControllerBase
{
    private readonly ISimulatorService _service;

    public SimulatorController(ISimulatorService simulatorService)
    {
        _service = simulatorService;
    }

    [HttpPost("simulate")]
    public async Task<ActionResult<CombatOutputCollection>> Simulate([FromBody] CombatRequest combatRequest)
    {
        var combatOutputCollection = await _service.Simulate(combatRequest.ToInput());
        return Ok(combatOutputCollection);
    }
}