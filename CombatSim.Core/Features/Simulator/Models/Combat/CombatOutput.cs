using System.Diagnostics;
using System.Text;

namespace CombatSim.Core.Features.Simulator.Models;

public class CombatOutput
{

    #region Constructor

    private readonly Stopwatch _stopwatch;
    
    public CombatOutput(int startCacheCount)
    {
        Report.StartCacheCount = startCacheCount;
        Report.StartTime = DateTime.UtcNow;
        _stopwatch = Stopwatch.StartNew();
    }

    #endregion

    #region Properties

    public List<RoundOutput> Rounds { get; set; } = new List<RoundOutput>();

    public CombatOutputReport Report { get; internal set; } = new CombatOutputReport();

    #endregion

    #region Internal Methods

    internal void FinalizeReport(bool didHeroesWin, int endCacheCount)
    {
        Report.TotalRounds = Rounds.Count;
        Report.DidHeroesWin = didHeroesWin;
        Report.EndCacheCount = endCacheCount;
        Report.EndTime = DateTime.UtcNow;
        Report.ElapsedMilliseconds = Math.Round(_stopwatch.Elapsed.TotalMilliseconds, 5);
    }

    #endregion

    #region Public Methods

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.Append("--------------------\n");
        sb.Append("--- Combat Start ---\n");
        sb.Append("--------------------\n");
        sb.Append("\n");

        foreach (var round in Rounds)
        {
            sb.Append($"Round #{round.RoundNumber}\n");
            sb.Append("\n");

            foreach (var attackResult in round.AttackResults)
            {
                sb.Append(attackResult.Log + "\n");
            }
            sb.Append("\n");
        }

        if (Report.DidHeroesWin)
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
        sb.Append($"Elapsed milliseconds: {Report.ElapsedMilliseconds}\n");
        sb.Append($"Starting cache count: {Report.StartCacheCount}\n");
        sb.Append($"Ending cache count: {Report.EndCacheCount}\n");
        sb.Append("\n");

        return sb.ToString();
    }

    #endregion

}