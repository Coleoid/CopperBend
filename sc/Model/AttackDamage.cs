namespace CopperBend.Model
{
    public class AttackDamage
    {
        public AttackDamage(int initial, string type)
        {
            Initial = Current = initial;
            Type = type;
        }

        public string Type { get; set; }
        public int Initial { get; set; }
        public int Current { get; set; }
    }
#pragma warning restore SA1402 // File may only contain a single type

}
