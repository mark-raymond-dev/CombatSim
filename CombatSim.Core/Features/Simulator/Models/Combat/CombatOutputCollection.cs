namespace CombatSim.Core.Features.Simulator.Models;

public class CombatOutputCollection : List<CombatOutput>
{

    #region Properties

    public int CacheCount { get; set; }
    public long ElapsedMilliseconds { get; set; }

    #endregion

    #region Public Methods

    public CombatOutputCollectionReport GetReport()
    {
        var report = new CombatOutputCollectionReport
        {
            TotalCombats = Count,
            HeroWinCount = this.Count(x => x.DidHeroesWin)
        };
        report.HeroLoseCount = report.TotalCombats - report.HeroWinCount;
        report.HeroWinPercentage = (decimal)report.HeroWinCount / report.TotalCombats;
        report.HeroLosePercentage = 1 - report.HeroWinPercentage;
        report.AverageRoundsPerCombat = this.Average(x => x.Rounds.Count);
        report.CacheCount = this.CacheCount;
        return report;
    }

    #endregion

}