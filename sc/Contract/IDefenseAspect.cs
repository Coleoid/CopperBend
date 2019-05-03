namespace CopperBend.Contract
{
    public interface IDefenseAspect
    {
        IBeing Being { get; set; }
        void SetResistance(string damageType, double portionBlocked);
        int ApplyDamage(IDamage damage);
    }
}
