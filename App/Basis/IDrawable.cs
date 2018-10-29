using RLNET;

namespace CopperBend.App
{
    public interface IDrawable
    {
        RLColor ColorForeground { get; }
        char Symbol { get; }
    }
}
