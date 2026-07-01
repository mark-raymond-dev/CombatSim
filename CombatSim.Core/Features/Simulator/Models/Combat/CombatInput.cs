namespace CombatSim.Core.Features.Simulator.Models;

public class CombatInput
{
    public List<Creature> Creatures { get; set; } = new List<Creature>();

    public CombatInput Clone()
    {
        return new CombatInput
        {
            Creatures = Creatures.Select(c => c.Clone()).ToList()
        };
    }
}