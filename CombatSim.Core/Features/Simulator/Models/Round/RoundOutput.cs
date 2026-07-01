namespace CombatSim.Core.Features.Simulator.Models;

public class RoundOutput
{
    public int RoundNumber { get; set; } = 1;
    public List<AttackResult> AttackResults { get; set; } = new List<AttackResult>();
}