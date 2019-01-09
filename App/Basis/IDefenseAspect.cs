namespace CopperBend.App
{
    public interface IDefenseAspect
    {
        IActor Actor { get; set; }
        void SetResistance(string damageType, double portionBlocked);
        int ApplyDamage(IDamage damage);
    }
}
