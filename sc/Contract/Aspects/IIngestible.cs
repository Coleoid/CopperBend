namespace CopperBend.Contract
{
    public interface IIngestible : IUsable
    {
        uint PlantID { get; set; }
        string VerbPhrase { get; set; }
        bool IsFruit { get; set; }
        int FoodValue { get; set; }
        int TicksToEat { get; set; }
    }
}
