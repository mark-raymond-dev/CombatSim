using CombatSim.Api.Models;
using CombatSim.Core.Features.Simulator.Models;

namespace CombatSim.Api.Mapping;

public static class CombatRequestExtensions
{
    public static CombatInput ToInput(this CombatRequest request) => new()
    {
        Creatures = request.Creatures?.Select(c => c.ToInput()).ToList() ?? new List<CreatureInput>(),
        SimulationCount = request.SimulationCount ?? 1,
        MillisecondsDelayBetweenRounds = request.MillisecondsDelayBetweenRounds ?? 0
    };
}