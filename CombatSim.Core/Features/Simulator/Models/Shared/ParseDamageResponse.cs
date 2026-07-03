namespace CombatSim.Core.Features.Simulator.Models;

public class ParseDamageResponse
{
    
    #region Properties

    public string ?OriginalExpression { get; set; }
    public int DamageDieCount { get; set; }
    public int DamageDieBase { get; set; }
    public int DamageModifier { get; set; }
    public string ?DamageType { get; set; }
    public string ?ParseStatus { get; set; }
    public double AverageDamage { get; set; }

    #endregion

}