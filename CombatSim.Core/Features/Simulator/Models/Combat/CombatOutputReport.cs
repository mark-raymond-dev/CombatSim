using System.Text;

namespace CombatSim.Core.Features.Simulator.Models;

public class CombatOutputReport
{

    #region Properties

    public int TotalRounds { get; internal set; }
    public bool DidHeroesWin { get; internal set; }
    public int StartCacheCount { get; internal set; }
    public int EndCacheCount { get; internal set; }
    public DateTime StartTime { get; internal set; }
    public DateTime EndTime { get; internal set; }
    public double ElapsedMilliseconds { get; internal set; }

    #endregion

    #region Public Methods

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("-------- SIMULATION REPORT --------\n");
        sb.Append($"Total rounds: {TotalRounds}\n");
        sb.Append($"Did heroes win: {DidHeroesWin}\n");
        sb.Append($"Starting cache count: {StartCacheCount}\n");
        sb.Append($"Ending cache count: {EndCacheCount}\n");
        sb.Append($"Start time: {StartTime:O}\n");
        sb.Append($"End time: {EndTime:O}\n");
        sb.Append($"Total elapsed milliseconds: {ElapsedMilliseconds:F5}\n");
        return sb.ToString();
    }

    #endregion

}