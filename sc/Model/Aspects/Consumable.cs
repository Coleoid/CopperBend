using CopperBend.Contract;

namespace CopperBend.Model.Aspects
{
    public class Consumable : IConsumable
    {
        public bool IsFruit { get; set; }
        public int FoodValue { get; set; }
        public int TicksToEat { get; set; }
        public (string Name, int Degree) Effect { get; set; }
        public uint PlantID { get; set; }
        public string ConsumeVerb { get; set; } = "eat";
    }
}
