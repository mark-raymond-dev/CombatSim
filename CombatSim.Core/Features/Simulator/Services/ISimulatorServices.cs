namespace CombatSim.Core.Features.Simulator.Services;

using CombatSim.Core.Features.Simulator.Models;

public interface ISimulatorService
{
    Task<CombatOutputCollection> Simulate(CombatInput combatInput);
}