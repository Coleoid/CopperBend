using System;
using System.Collections.Generic;
using CopperBend.App.Model;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine
    {
        private readonly RLRootConsole GameConsole;
        private readonly Scheduler Scheduler;
        private readonly Queue<RLKeyPress> InputQueue;

        public CommandDispatcher Dispatcher { get; }
        public IAreaMap Map { get; private set; }
        public Actor Player;

        public GameEngine(RLRootConsole console)
        {
            GameConsole = console;

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
        }

        public void Run()
        {
            if (Player == null) throw new Exception("Must have Player before starting engine.");
            if (Map == null) throw new Exception("Must have Map before starting engine.");

            Dispatcher.Init(Map, Player);

            GameConsole.Update += onUpdate;
            GameConsole.Render += onRender;
            GameConsole.Run();
        }

        private void onRender(object sender, UpdateEventArgs e)
        {
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

            Dispatcher.Next();
        }
    }
}
