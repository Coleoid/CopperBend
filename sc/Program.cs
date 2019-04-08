using Game = SadConsole.Game;

namespace CbRework
{
    public static class Program
    {
        static void Main()
        {
            int windowWidth = 80;
            int windowHeight = 40;
            Game.Create(windowWidth, windowHeight);
            Game.OnInitialize = () => new Engine(windowWidth, windowHeight);
            //  Cannot create a Console until Game.Init time, after the
            //  .Run below.

            Game.Instance.Run();
            
            Game.Instance.Dispose();
        }
    }
}
