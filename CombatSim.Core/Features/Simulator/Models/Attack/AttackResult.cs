namespace CombatSim.Core.Features.Simulator.Models;

public class AttackResult
{
    public string AttackerName { get; set; } = "";
    public string DefenderName { get; set; } = "";
    public int D20 { get; set; }
    public DegreeOfSuccess DegreeOfSuccess { get; set; } = DegreeOfSuccess.CriticalMiss;
    public int Damage { get; set; }
    public string Log { get; set; } = "";
}