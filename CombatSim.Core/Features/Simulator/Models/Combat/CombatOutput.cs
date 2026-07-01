using System.Text;

namespace CombatSim.Core.Features.Simulator.Models;

public class CombatOutput
{

    #region Properties

    public List<RoundOutput> Rounds { get; set; } = new List<RoundOutput>();
    public bool DidHeroesWin { get; set; }

    #endregion

    #region Public Methods

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.Append("--------------------\n");
        sb.Append("--- Combat Start ---\n");
        sb.Append("--------------------\n");
        sb.Append("\n");

        foreach (var round in this.Rounds)
        {
            sb.Append($"Round #{round.RoundNumber}\n");
            sb.Append("\n");

            foreach (var attackResult in round.AttackResults)
            {
                sb.Append(attackResult.Log + "\n");
            }
            sb.Append("\n");
        }

        if (this.DidHeroesWin)
        {
            sb.Append("The heroes won !!!\n");
        }
        else
        {
            sb.Append("The monsters won ...\n");
        }
        sb.Append("\n");

        sb.Append("------------------\n");
        sb.Append("--- Combat End ---\n");
        sb.Append("------------------\n");
        sb.Append("\n");

        return sb.ToString();
    }

    #endregion

}