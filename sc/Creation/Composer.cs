using System;
using System.Drawing;
using log4net;
using SC_Game = SadConsole.Game;
using XNA_Game = Microsoft.Xna.Framework.Game;
using CopperBend.Logic;
using CopperBend.Model;

namespace CopperBend.Creation
{
    public class Composer
    {
        private string InitialSeed { get; set; }
        private ILog Logger { get; set; }

        private XNA_Game game;
        private int gameWidth;
        private int gameHeight;

        public void Compose(string seed, bool testMode)  // 1st
        {
            InitialSeed = seed;

            try
            {
                game = CreateGameInstance();  //  2nd
            }
            catch (Exception ex)
            {
                Logger.Fatal("Exception terminated construction", ex);
                return;
            }

            SourceMe.Build(new Size(gameWidth, gameHeight));  //  3rd
            Logger = SourceMe.The<ILog>();
            Basis.ConnectIDGenerator();
        }

        public XNA_Game CreateGameInstance()  //  2nd
        {
            gameWidth = 160;
            gameHeight = 60;

            SC_Game.Create(gameWidth, gameHeight);

            // 0.N:  Engine isa SadConsole.ContainerConsole
            // This inheritance constraint causes some twisting.
            SC_Game.OnInitialize = InitializeEngine;  //  Called when game.Run()
            SC_Game.Instance.Window.Title = "Copper Bend";

            return SC_Game.Instance;
        }

        public void InitializeEngine()  //  Nth + 2
        {
            var engine = SourceMe.The<Engine>();
            engine.Init(InitialSeed);
        }


        public void LaunchGame()  //  Nth
        {
            try
            {
                game.Run();  //  Nth + 1
            }
            catch (Exception ex)
            {
                Logger.Fatal("Exception terminated run", ex);
            }
        }


        public void Release()
        {
            SC_Game.Instance?.Dispose();
        }
    }
}
