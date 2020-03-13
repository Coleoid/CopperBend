using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Attack : IAttack
    {
        public IAttacker Attacker { get; set; }
        public IAttackMethod AttackMethod { get; set; }
        public IDefender Defender { get; set; }
        public IDefenseMethod DefenseMethod { get; set; }
    }
}
