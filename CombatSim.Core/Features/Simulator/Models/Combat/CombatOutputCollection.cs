using System.Diagnostics;

namespace CombatSim.Core.Features.Simulator.Models;

public class CombatOutputCollection
{

    #region Constructor

    private readonly Stopwatch _stopwatch;
    
    public CombatOutputCollection(int startCacheCount)
    {
        Report.StartCacheCount = startCacheCount;
        Report.StartTime = DateTime.UtcNow;
        _stopwatch = Stopwatch.StartNew();
    }

    #endregion

    #region Properties

    public List<CombatOutput> Combats { get; set; } = new List<CombatOutput>();
    
    public CombatOutputCollectionReport Report { get; internal set; } = new CombatOutputCollectionReport();

    #endregion

    #region Internal Methods

    internal void FinalizeReport(int endCacheCount)
    {
        Report.TotalCombats = Combats.Count;
        Report.HeroWinCount = Combats.Count(x => x.Report.DidHeroesWin);        
        Report.HeroLoseCount = Report.TotalCombats - Report.HeroWinCount;
        Report.HeroWinPercentage = (decimal)Report.HeroWinCount / Report.TotalCombats;
        Report.HeroLosePercentage = 1 - Report.HeroWinPercentage;
        Report.AverageRoundsPerCombat = Combats.Average(x => x.Rounds.Count);
        Report.EndCacheCount = endCacheCount;
        Report.EndTime = DateTime.UtcNow;
        Report.ElapsedMilliseconds = Math.Round(_stopwatch.Elapsed.TotalMilliseconds, 5);
    }

    #endregion

}