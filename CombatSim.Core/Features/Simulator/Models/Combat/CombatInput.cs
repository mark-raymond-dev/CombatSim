namespace CombatSim.Core.Features.Simulator.Models;

public class CombatInput
{
    public List<CreatureInput> Creatures { get; set; } = new List<CreatureInput>();

    public int SimulationCount { get; set; } = 1;

    public int MillisecondsDelayBetweenRounds { get; set; } = 0;

    public ReturnType ReturnType { get; set; } = ReturnType.SummaryReport;

    public CombatInput Clone()
    {
        return new CombatInput
        {
            Creatures = Creatures.Select(c => c.Clone()).ToList(),
            SimulationCount = SimulationCount,
            MillisecondsDelayBetweenRounds = MillisecondsDelayBetweenRounds,
            ReturnType = ReturnType
        };
    }
}