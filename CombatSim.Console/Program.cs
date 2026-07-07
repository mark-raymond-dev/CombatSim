using CombatSim.Core.Features.Simulator.Models;
using CombatSim.Core.Features.Simulator.Services;

var combatInput = new CombatInput
{
    Creatures =
    [
        new() { IsHero = false, Order = 1, Name = "Troll1", HP = 150, AC = 20, ToHit = 10, Damage = "2d8+6 bludgeoning"},
        new() { IsHero = false, Order = 2, Name = "Troll2", HP = 150, AC = 20, ToHit = 10, Damage = "2d8+6 bludgeoning"},
        new() { IsHero = false, Order = 3, Name = "Troll3", HP = 150, AC = 20, ToHit = 10, Damage = "2d8+6 bludgeoning"},
        new() { IsHero = true, Order = 4, Name = "Kelp", HP = 120, AC = 22, ToHit = 10, Damage = "2d10+2 slashing"},
        new() { IsHero = true, Order = 5, Name = "Tabar", HP = 90, AC = 19, ToHit = 11, Damage = "2d8+6 piercing"},
        new() { IsHero = true, Order = 6, Name = "Avatar", HP = 70, AC = 16, ToHit = 9, Damage = "4d6 fire"},
        new() { IsHero = true, Order = 7, Name = "Crichton", HP = 110, AC = 19, ToHit = 10, Damage = "2d12+2 slashing"},
    ]
};

bool doMonteCarlo = true;
var httpClient = new HttpClient();
var cache = new Dictionary<string, ParseDamageResponse>();
var service = new SimulatorService(httpClient, cache);
if (doMonteCarlo)
{
    combatInput.SimulationCount = 1000;
    var combatOutputCollection = await service.Simulate(combatInput);
    var report = combatOutputCollection.GetReport();
    Console.WriteLine(report);
}
else
{
    var combatOutputCollection = await service.Simulate(combatInput);
    var combatOutput = combatOutputCollection[0];
    Console.WriteLine(combatOutput);
}