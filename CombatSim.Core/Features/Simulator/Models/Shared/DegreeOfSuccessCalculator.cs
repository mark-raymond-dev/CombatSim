namespace CombatSim.Core.Features.Simulator.Models;

public static class DegreeOfSuccessCalculator
{
    public static DegreeOfSuccess GetDegreeOfSuccess(int toHit, int d20, int defense, bool natural20Upgrades = true, bool natural1Downgrades = true)
    {
        // Determine total attack roll and target numbers for each degree of success.
        int total = d20 + toHit;
        int tgtCritHit = defense + 10;
        int tgtNormHit = defense;
        int tgtNormMiss = defense - 10;

        // Determine the base degree of success.
        var degreeOfSuccess = DegreeOfSuccess.CriticalMiss; // default
        if (total >= tgtCritHit)
        {
            degreeOfSuccess = DegreeOfSuccess.CriticalHit;
        }
        else if (total >= tgtNormHit)
        {
            degreeOfSuccess = DegreeOfSuccess.Hit;
        }
        else if (total >= tgtNormMiss)
        {
            degreeOfSuccess = DegreeOfSuccess.Miss;
        }

        // Apply Nat 20 / 1 rule adjustments (if applicable).
        if (natural20Upgrades && d20 == 20 && degreeOfSuccess < DegreeOfSuccess.CriticalHit) degreeOfSuccess++; // Upgrade on Natural 20
        if (natural1Downgrades && d20 == 1 && degreeOfSuccess > DegreeOfSuccess.CriticalMiss)  degreeOfSuccess--; // Downgrade on Natural 1

        return degreeOfSuccess;
    }
}