using System.ComponentModel.DataAnnotations;

namespace CombatSim.Api.Models;

public class CreatureRequest
{    
    [Required(ErrorMessage = "Creature must have an order assigned.")]
    [Range(1, 100, ErrorMessage = "Creature order must be between 1 and 100.")]
    public int? Order { get; set; }
    
    [Required(ErrorMessage = "Creature must have a name.")]
    public string? Name { get; set;}
    
    [Required(ErrorMessage = "Creature must have HP assigned.")]
    [Range(1, 1000, ErrorMessage = "Creature HP must be between 1 and 1000.")]
    public int? HP { get; set; }
    
    [Required(ErrorMessage = "Creature must have AC assigned.")]
    [Range(0, 100, ErrorMessage = "Creature AC must be between 0 and 100.")]
    public int? AC { get; set; }

    [Required(ErrorMessage = "Creature must have a Damage Expression assigned.")]
    public string? Damage { get; set; }
    
    [Required(ErrorMessage = "Creature must be identified as a hero or not.")]
    public bool? IsHero { get; set; }
    
    [Required(ErrorMessage = "Creature must have a ToHit value assigned.")]
    [Range(0, 100, ErrorMessage = "Creature ToHit must be between 0 and 100.")]
    public int? ToHit { get; set; }
}