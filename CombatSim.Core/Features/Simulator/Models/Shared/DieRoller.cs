namespace CombatSim.Core.Features.Simulator.Models;

public static class DieRoller
{
    private static readonly Random random = new();
    public static int Roll(int dieSize, int dieCount = 1, int modifier = 0)
    {
        int total = 0;
        for (var i = 0; i < dieCount; i++)
        {
            int roll = SimpleRoll(dieSize);
            total += roll;
        }
        total += modifier;
        return total;
    }

    public static int SimpleRoll(int dieSize)
    {
        int min = 1; // inclusive
        int max = dieSize + 1; // exclusive
        int roll = random.Next(min, max);
        return roll;
    }
}