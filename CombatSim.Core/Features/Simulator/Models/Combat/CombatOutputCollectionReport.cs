using System.Text;

namespace CombatSim.Core.Features.Simulator.Models;

public class CombatOutputCollectionReport
{

    #region Properties

    public int TotalCombats { get; set; }
    public int HeroWinCount { get; set; }
    public int HeroLoseCount { get; set; }
    public decimal HeroWinPercentage { get; set; }
    public decimal HeroLosePercentage { get; set; }
    public double AverageRoundsPerCombat { get; set; }

    #endregion

    #region Public Methods

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("-------- SIMULATION REPORT --------\n");
        sb.Append($"Total simulated combats: {TotalCombats}\n");
        sb.Append($"Hero win count: {HeroWinCount}\n");
        sb.Append($"Hero lose count: {HeroLoseCount}\n");
        sb.Append($"Hero win percentage: {HeroWinPercentage:P3}\n");
        sb.Append($"Hero lose percentage: {HeroLosePercentage:P3}\n");
        sb.Append($"Average rounds per combat: {AverageRoundsPerCombat:F3}\n");
        return sb.ToString();
    }

    #endregion

}