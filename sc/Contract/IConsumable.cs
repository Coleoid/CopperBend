namespace CopperBend.Contract
{
    public interface IConsumable
    {
        bool IsFruit { get; set; }
        int FoodValue { get; set; }
        int TicksToEat { get; set; }
        (string Name, int Degree) Effect { get; set; }
        uint PlantID { get; set; }
        string ConsumeVerb { get; set; }
    }
}
