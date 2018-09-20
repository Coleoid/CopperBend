using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.App.Model;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine
    {
        public RLRootConsole GameConsole;
        internal IAreaMap Map;
        internal Actor Player;

        public void Init(RLRootConsole console)
        {
            if (GameConsole != null)
                throw new Exception("Second call to Init().");

            GameConsole = console;
            GameConsole.Update += onUpdate;
            GameConsole.Render += onRender;
        }

        public void Run()
        {
            GameConsole.Run();
        }

        private void onRender(object sender, UpdateEventArgs e)
        {
            //TODO: better spot, from scheduler
            ActOnMap(Map);

            //  if no panel needs an update, why render?
            if (!_displayDirty) return;
            GameConsole.Clear();


            Map.DrawMap(GameConsole);

            GameConsole.Draw();
            _displayDirty = false;
        }

        private void ActOnMap(IAreaMap map)
        {
            //0.1
            if (PlayerMoveQueue.Any())
            {
                var player = map.Actors[0];
                var direction = PlayerMoveQueue.Dequeue();
                var newX = player.X;
                var newY = player.Y;
                if (direction == Direction.Up) newY--;
                if (direction == Direction.Down) newY++;
                if (direction == Direction.Left) newX--;
                if (direction == Direction.Right) newX++;

                if (map.SetActorPosition(player, newX, newY))
                {
                    _displayDirty = true;
                }
            }
        }

        private bool _displayDirty = true;

        private void onUpdate(object sender, UpdateEventArgs e)
        {
            //  For the short term, we only care about the keyboard
            RLKeyPress key = GameConsole.Keyboard.GetKeyPress();

            if (key != null)
            {
                KeyboardCommand(key);
            }

            Proceed();
        }

        private bool _readyForUserCommand = true;
        private bool _actionChosen = false;

        public void Proceed()
        {
            if (_readyForUserCommand)
            {
                if (_actionChosen)
                {
                    PerformAction();  // which sets _ready depending on command.  (i)nventory we remain ready, attack takes time
                }
            }

            if (!_readyForUserCommand)
            {
                //  work the schedule
                 
            }


            //FUTURE:  advance background animation in here somewhere
        }

        private void KeyboardCommand(RLKeyPress keyPress)
        {
            var direction =
                keyPress.Key == RLKey.Up ? Direction.Up :
                keyPress.Key == RLKey.Down ? Direction.Down :
                keyPress.Key == RLKey.Left ? Direction.Left :
                keyPress.Key == RLKey.Right ? Direction.Right :
                Direction.None;

            //  Preparing decoupled move
            if (direction != Direction.None)
            {
                QueuePlayerMove(direction);
            }
        }

        private Queue<Direction> PlayerMoveQueue = new Queue<Direction>();
        private void QueuePlayerMove(Direction direction)
        {
            PlayerMoveQueue.Enqueue(direction);
        }
    }
}
