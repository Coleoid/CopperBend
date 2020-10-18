namespace CopperBend.Contract
{
    public interface IAttack
    {
        IAttacker Attacker { get; set; }
        IAttackMethod AttackMethod { get; set; }
        IDefender Defender { get; set; }
        IDefenseMethod DefenseMethod { get; set; }
    }
}
