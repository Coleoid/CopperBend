using System;
using System.Collections.Generic;
using CopperBend.App.Basis;
using CopperBend.App.Model;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine : IGameState
    {
        private readonly RLRootConsole GameConsole;
        private readonly Scheduler Scheduler;
        private readonly Queue<RLKeyPress> InputQueue;

        public CommandDispatcher Dispatcher { get; }
        public IAreaMap Map { get; private set; }
        public IActor Player { get; private set; }

        public GameEngine(RLRootConsole console, Actor player)
        {
            GameConsole = console;
            Player = player;

            InputQueue = new Queue<RLKeyPress>();
            Scheduler = new Scheduler();
            Dispatcher = new CommandDispatcher(InputQueue, Scheduler);
        }

        public void LoadMap(IAreaMap map)
        {
            Map = map;
            foreach (var actor in map.Actors)
            {
                Scheduler.Add(new ScheduleEntry(12, actor));
            }

            map.Actors.Add(Player);
            map.UpdatePlayerFieldOfView(Player);
        }

        public void Run()
        {
            if (Player == null) throw new Exception("Must have Player before starting engine.");
            if (Map == null) throw new Exception("Must have Map before starting engine.");

            Dispatcher.Init(this);

            GameConsole.Update += onUpdate;
            GameConsole.Render += onRender;
            GameConsole.Run();
        }

        private void onRender(object sender, UpdateEventArgs e)
        {
            //FUTURE:  real-time (background) animation around here

            //  If the map hasn't changed, why render?
            if (!Map.DisplayDirty) return;

            GameConsole.Clear();
            Map.DrawMap(GameConsole);
            GameConsole.Draw();
            Map.DisplayDirty = false;
        }

        private void onUpdate(object sender, UpdateEventArgs e)
        {
            //  For now, only checking the keyboard for input
            RLKeyPress key = GameConsole.Keyboard.GetKeyPress();
            if (key != null)
            {
                if (key.Alt && key.Key == RLKey.F4)
                {
                    GameConsole.Close();
                    return;
                }

                InputQueue.Enqueue(key);
            }

            ActOnMode();
        }

        public GameMode Mode { get; set; }
        private void ActOnMode()
        {
            switch (Mode)
            {
            //  A game menu will block even pending messages 
            case GameMode.MenuOpen:
                HandleMenus();
                break;

            //  Messages waiting for the player block player input and scheduled events
            case GameMode.MessagesPending:
                Dispatcher.HandlePendingMessages();
                break;

            //  Waiting for player actions blocks Scheduler
            case GameMode.PlayerReady:
                Dispatcher.HandlePlayerCommands();
                break;

            //  When the player has committed to a slow action, everything happens
            case GameMode.Schedule:
                Scheduler.DoNext(this);
                break;

            case GameMode.Unknown:
                throw new Exception("Game mode unknown, perhaps Init() was missed.");

            default:
                throw new Exception($"Game mode [{Mode}] not written yet.");
            }
        }

        private void HandleMenus()
        {
            //TODO:  All these:
            //  Start new game
            //  Load game
            //  Save and Quit
        }
    }
}
