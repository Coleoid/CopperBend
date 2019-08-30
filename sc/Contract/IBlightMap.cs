using GoRogue;

namespace CopperBend.Contract
{
    public interface IBlightMap
    {
        string Name { get; set; }
        void AddItem(IAreaBlight blight, Coord coord);
        IAreaBlight GetItem(Coord coord);
        void RemoveItem(IAreaBlight blight);
    }
}
