using System;
using System.Collections.Generic;
using System.Linq;
using RLNET;

namespace CopperBend.App
{
    public class GameLoop
    {
        public RLRootConsole GameConsole;
        internal IcbMap Map;
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
            GameConsole.Clear();

            ActOnMap(Map);
            Map.Draw(GameConsole);

            GameConsole.Draw();
        }

        private void ActOnMap(IcbMap map)
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
                    //??
                }
            }
        }

        private void onUpdate(object sender, UpdateEventArgs e)
        {
            //  For the short term, we only care about the keyboard
            RLKeyPress key = GameConsole.Keyboard.GetKeyPress();
            if (key == null) return;

            PlayerMove_OnKeyPress(key);
        }


        private void PlayerMove_OnKeyPress(RLKeyPress keyPress)
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
