using Game = SadConsole.Game;

namespace CbRework
{
    public static class Program
    {
        static void Main()
        {
            Game.Create(60, 30);

            var engine = new Engine();
            Game.OnInitialize = engine.Init;
            Game.OnUpdate = engine.Update;

            Game.Instance.Run();
            Game.Instance.Dispose();
        }

    }
}
