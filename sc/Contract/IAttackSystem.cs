using System.Collections.Generic;
using CopperBend.Model;

namespace CopperBend.Contract
{
    public interface IAttackSystem
    {
        Queue<Attack> AttackQueue { get; }
        Queue<IDelible> Destroyed { get; }
        IControlPanel Panel { get; set; }
        IRotMap RotMap { get; }

        void AddAttack(Attack attack);
        void AddAttack(IAttacker attacker, IAttackMethod attack, IDefender defender, IDefenseMethod defense);
        string AttackMessage(Attack attack);
        void CheckForSpecials(Attack attack);
        IEnumerable<IAreaRot> NeighborRotsOf(IAreaRot areaRot);
        void ReapDestroyed();
        void RegisterDamage(IDelible target, IEnumerable<AttackDamage> damages);
        IEnumerable<AttackDamage> ResistDamages(IEnumerable<AttackDamage> damages, IDefenseMethod defense);
        void ResolveAttack(Attack attack);
        void ResolveAttackQueue();
        int RollDamage(IAttackEffect effect);
        IEnumerable<AttackDamage> RollDamages(IAttackMethod attack);
    }
}
