namespace CombatSim.Core.Features.Simulator.Models;

public class Creature
{
    
    #region Properties

    public bool IsHero { get; set; } = false; // True = Hero, False = Monster (this is how we split up creatures into parties)
    public int Order { get; set; } = 0;
    public string Name { get; set;} = "";
    public int HP { get; set; }
    public int AC { get; set; }
    public int ToHit { get; set; }
    public string Damage { get; set; } = "";

    #endregion

    #region Public Methods

    public Creature Clone()
    {
        return new Creature
        {
            IsHero = IsHero,
            Order = Order,
            Name = Name,
            HP = HP,
            AC = AC,
            ToHit = ToHit,
            Damage = Damage
        };
    }

    #endregion
    
}