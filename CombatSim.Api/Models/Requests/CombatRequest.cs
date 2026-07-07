using System.ComponentModel.DataAnnotations;
using CombatSim.Core.Features.Simulator.Models;

namespace CombatSim.Api.Models;

public class CombatRequest : IValidatableObject
{

    #region Properties

    [Required(ErrorMessage = "Creature collection is required.")]
    public List<CreatureRequest>? Creatures { get; set; }

    [Required(ErrorMessage = "Simulation count is required.")]
    [Range(1, 100000, ErrorMessage = "Simulation count must be between 1 and 100000.")]
    public int? SimulationCount { get; set; }

    [Range(0, 1000, ErrorMessage = "Milliseconds delay between rounds must be between 0 and 1000.")]
    public int MillisecondsDelayBetweenRounds { get; set; }

    [Required(ErrorMessage = "Return type is required.")]
    public ReturnType? ReturnType { get; set; }

    #endregion

    #region Validation

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // At least one hero is required.
        var heroCount = Creatures?.Count(c => c.IsHero == true) ?? 0;
        if (heroCount == 0)
            yield return new ValidationResult(
                "At least one hero is required.",
                new[] { nameof(Creatures) });

        // At least one monster is required.
        var monsterCount = Creatures?.Count(c => c.IsHero == false) ?? 0;
        if (monsterCount == 0)
            yield return new ValidationResult(
                "At least one monster is required.",
                new[] { nameof(Creatures) });
    }  

    #endregion

}