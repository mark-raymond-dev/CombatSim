namespace CombatSim.Core.Features.Simulator.Models;

public class CombatOutputCollection : List<CombatOutput>
{
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
        return report;
    }
}