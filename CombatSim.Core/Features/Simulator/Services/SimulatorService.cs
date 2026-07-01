using CombatSim.Core.Features.Simulator.Models;

namespace CombatSim.Core.Features.Simulator.Services;

public class SimulatorService
{

    #region Private Methods

    private int AliveCount(CombatInput combatInput, bool isHero)
    {
        // Counts how many creatures are alive
        // on the side indicated by isHero.
        return combatInput.Creatures
            .Where(p => p.IsHero == isHero)
            .Count(p => p.HP > 0);
    }

    private bool IsContinueCombat(CombatInput combatInput)
    {
        // We want to continue combat as long as there is at
        // least one creature on each side that is "alive".
        int heroesAlive = AliveCount(combatInput, isHero: true);
        int monstersAlive = AliveCount(combatInput, isHero: false);
        bool continueCombat = heroesAlive > 0 && monstersAlive > 0;
        return continueCombat;
    }

    private int RollDamage(Creature attacker, DegreeOfSuccess degreeOfSuccess)
    {
        // For now, we will hard code the die size to 6, the die count to 2, 
        // and the modifier to 3, essentially giving us:  2d6+3
        int baseDamage = DieRoller.Roll(6, 2, 3);
        int actualDamage = degreeOfSuccess switch
        {
            DegreeOfSuccess.CriticalMiss => 0,
            DegreeOfSuccess.Miss => 0,
            DegreeOfSuccess.Hit => baseDamage,
            DegreeOfSuccess.CriticalHit => baseDamage * 2,
            _ => throw new NotImplementedException()
        };        
        return actualDamage;
    }

    private string GetAttackLog(Creature attacker, Creature defender, DegreeOfSuccess degreeOfSuccess, int damage)
    {
        int hp1 = defender.HP;
        int hp2 = hp1 - damage;
        if (hp2 < 0) hp2 = 0;
        string dead = hp2 == 0 ? " ..... DEAD" : "";
        string defHp = $"(HP: {hp1} => {hp2}{dead})";
        string result = degreeOfSuccess switch
        {
            DegreeOfSuccess.CriticalMiss => "but critically missed ...",
            DegreeOfSuccess.Miss => "but missed.",
            DegreeOfSuccess.Hit => $"and hit for {damage} damage! {defHp}",
            DegreeOfSuccess.CriticalHit => $"and critically hit for {damage} damage!!! {defHp}",
            _ => throw new NotImplementedException()
        };
        var log = $"{attacker.Name} attacked {defender.Name}, {result}";
        return log;
    }
    
    private AttackResult ProcessAttack(Creature attacker, Creature defender)
    {
        int d20 = DieRoller.SimpleRoll(20);
        var degreeOfSuccess = DegreeOfSuccessCalculator.GetDegreeOfSuccess(
            attacker.ToHit, d20, defender.AC,
            natural20Upgrades: true, natural1Downgrades: true);
        int damage = RollDamage(attacker, degreeOfSuccess);
        var log = GetAttackLog(attacker, defender, degreeOfSuccess, damage);
        return new AttackResult
        {
            AttackerName = attacker.Name,
            DefenderName = defender.Name,
            D20 = d20,
            DegreeOfSuccess = degreeOfSuccess,
            Damage = damage,
            Log = log
        };
    }

    #endregion

    #region Public Methods

    public CombatOutputCollection FightMultiple(CombatInput combatInput, int count = 1)
    {
        var combatOutputCollection = new CombatOutputCollection();

        for (var i = 0; i < count; i++)
        {
            var combatInputClone = combatInput.Clone();
            var combatOutput = Fight(combatInputClone);
            combatOutputCollection.Add(combatOutput);
        }

        return combatOutputCollection;
    }

    public CombatOutput Fight(CombatInput combatInput)
    {
        var combatOutput = new CombatOutput();
        int roundNumber = 0;
        bool continueCombat = IsContinueCombat(combatInput);
        while (continueCombat)
        {
            roundNumber++;
            var roundOutput = new RoundOutput { RoundNumber = roundNumber };
            var aliveCreaturesOrdered = combatInput.Creatures
                .Where(p => p.HP > 0)
                .OrderBy(p => p.Order)
                .ThenBy(p => p.Name)
                .ToList();
            foreach (var attacker in aliveCreaturesOrdered)
            {
                if (attacker.HP > 0)
                {
                    var defender = aliveCreaturesOrdered
                        .FirstOrDefault(p => p.IsHero == !attacker.IsHero && p.HP > 0);
                    if (defender != null)
                    {
                        var attackResult = ProcessAttack(attacker, defender);
                        defender.HP -= attackResult.Damage;
                        roundOutput.AttackResults.Add(attackResult);
                    }                    
                }
            }
            combatOutput.Rounds.Add(roundOutput);
            continueCombat = IsContinueCombat(combatInput);
        }

        int heroAliveCount = AliveCount(combatInput, isHero: true);
        combatOutput.DidHeroesWin = heroAliveCount > 0;
        return combatOutput;
    }

    #endregion

}