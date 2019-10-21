namespace CopperBend.Contract
{
    public interface IIngestible : IUsable
    {
        uint PlantID { get; set; }
        bool IsFruit { get; set; }
    }
}
