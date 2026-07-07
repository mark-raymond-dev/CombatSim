using CombatSim.Api.Models;
using CombatSim.Core.Features.Simulator.Models;

namespace CombatSim.Api.Mapping;

public static class CreatureRequestExtensions
{
    public static CreatureInput ToInput(this CreatureRequest request) => new()
    {
        Order = request.Order ?? 0,
        Name = request.Name ?? "",
        HP = request.HP ?? 0,
        AC = request.AC ?? 0,
        Damage = request.Damage ?? "",
        IsHero = request.IsHero ?? true,
        ToHit = request.ToHit ?? 0
    };
}