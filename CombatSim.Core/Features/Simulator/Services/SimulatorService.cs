using CombatSim.Core.Features.Simulator.Models;
using System.Net.Http.Json;
using System.Diagnostics;

namespace CombatSim.Core.Features.Simulator.Services;

public class SimulatorService
{

    #region Private Properies (Injections)

    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, ParseDamageResponse> _cache;

    #endregion

    #region Constructors

    public SimulatorService(HttpClient httpClient, Dictionary<string, ParseDamageResponse> cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    #endregion

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
    
    private string GetApiCallExceptionSuffix(HttpResponseMessage response)
    {
        return $"URL='{response.RequestMessage?.RequestUri}', StatusCode={response.StatusCode}, ReasonPhrase={response.ReasonPhrase}";
    }

    #endregion

    #region Private Async Methods

    private async Task<ParseDamageResponse> ParseDamage(string damageExpression)
    {
        // Check if the result is already cached
        if (_cache.TryGetValue(damageExpression, out var cachedResult)) return cachedResult!;

        // Call the API to parse the damage expression. Retry up to 3 times if we get a "Too Many Requests" response.
        var apiBase = "https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net";
        var apiUrlGet = $"{apiBase}/api/v1/ParseDamage/calculate?Expression={Uri.EscapeDataString(damageExpression)}";
        int maxRetries = 3;
        var response = await _httpClient.GetAsync(apiUrlGet);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new Exception($"Something went wrong with API call: {GetApiCallExceptionSuffix(response)}");
            }

            for (int retry = 1; retry <= maxRetries; retry++)
            {
                await Task.Delay(300 * retry); // Exponential backoff
                response = await _httpClient.GetAsync(apiUrlGet);
                if (response.IsSuccessStatusCode) break;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API call failed after {maxRetries} retries: {GetApiCallExceptionSuffix(response)}");
            }
        }

        // Deserialize the response into a ParseDamageResponse object, then cache and return it.
        var result = await response.Content.ReadFromJsonAsync<ParseDamageResponse>()
            ?? throw new Exception($"Failed to parse damage expression: '{damageExpression}'");
        _cache[damageExpression] = result;
        return result;
    }

    private async Task<int> RollDamage(Creature attacker, DegreeOfSuccess degreeOfSuccess)
    {
        var result = await ParseDamage(attacker.Damage);
        
        int baseDamage = DieRoller.Roll(result.DamageDieBase, result.DamageDieCount, result.DamageModifier);
        
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

    private async Task<AttackResult> ProcessAttack(Creature attacker, Creature defender)
    {
        int d20 = DieRoller.SimpleRoll(20);
        var degreeOfSuccess = DegreeOfSuccessCalculator.GetDegreeOfSuccess(
            attacker.ToHit, d20, defender.AC,
            natural20Upgrades: true, natural1Downgrades: true);
        int damage = await RollDamage(attacker, degreeOfSuccess);
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

    #region Public Async Methods

    public async Task<CombatOutputCollection> FightMultiple(CombatInput combatInput, int count = 1)
    {
        var combatOutputCollection = new CombatOutputCollection();

        for (var i = 0; i < count; i++)
        {
            var combatInputClone = combatInput.Clone();
            var combatOutput = await Fight(combatInputClone);
            combatOutputCollection.Add(combatOutput);
        }

        combatOutputCollection.CacheCount = _cache.Count;
        return combatOutputCollection;
    }

    public async Task<CombatOutput> Fight(CombatInput combatInput, int millisecondsDelayBetweenRounds = 0)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
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
                        var attackResult = await ProcessAttack(attacker, defender);
                        defender.HP -= attackResult.Damage;
                        roundOutput.AttackResults.Add(attackResult);
                    }                    
                }
            }
            combatOutput.Rounds.Add(roundOutput);
            continueCombat = IsContinueCombat(combatInput);
            if (continueCombat && millisecondsDelayBetweenRounds > 0)
            {
                // Add a small delay to avoid overwhelming the API with requests.
                // NOTE: We use "await Task.Delay" instead of "Thread.Sleep" to avoid blocking the thread.
                await Task.Delay(millisecondsDelayBetweenRounds);
            }
        }

        int heroAliveCount = AliveCount(combatInput, isHero: true);
        combatOutput.DidHeroesWin = heroAliveCount > 0;
        stopwatch.Stop();
        combatOutput.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        combatOutput.CacheCount = _cache.Count;
        return combatOutput;
    }

    #endregion

}