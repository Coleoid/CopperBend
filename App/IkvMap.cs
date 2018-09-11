namespace CopperBend.App
{
    public interface IkvMap
    {
        int Width { get; }
        int Height { get; }
        TerrainType [,] Terrain { get; set; }
    }

}
