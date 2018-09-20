using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.App.Model;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine
    {
        private readonly RLRootConsole GameConsole;
        private readonly Scheduler Scheduler;
        private readonly Queue<RLKeyPress> InputQueue;
        private readonly Queue<Direction> PlayerMoveQueue;

        public IAreaMap Map;
        public Actor Player;

        //  True when the console needs redrawing
        //  Eventually this flag will be first class on the map and
        //  other panels, and the render loop will |= into a local
        private bool _displayDirty = true;

        private bool _readyForUserInput = true;
        //  After quick commands (e.g., inventory) we can act again,
        //  so _readyForUserInput stays true.
        //  After attack or move, the player cannot act again for
        //  a while, so they are put into the schedule and
        //  _readyForUserInput is false.
        //  Another case, multi-key commands, will leave _ready true.


        public GameEngine(RLRootConsole console)
        {
            GameConsole = console;
            GameConsole.Update += onUpdate;
            GameConsole.Render += onRender;

            Scheduler = new Scheduler();
            InputQueue = new Queue<RLKeyPress>();
            PlayerMoveQueue = new Queue<Direction>();
        }

        public void Run()
        {
            GameConsole.Run();
        }

        private void onRender(object sender, UpdateEventArgs e)
        {
            //  If the map hasn't changed, why render?
            if (!_displayDirty) return;

            GameConsole.Clear();
            Map.DrawMap(GameConsole);
            GameConsole.Draw();
            _displayDirty = false;
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

            while (_readyForUserInput && InputQueue.Any())
            {
                HandlePlayerCommands();
            }

            //  if the player is on the schedule, work the schedule
            while (!_readyForUserInput)
            {
                var nextUp = Scheduler.GetNext();

                //  The scheduled event is called here
                var newEvent = nextUp.Action();
                //  ...which may immediately schedule another event
                if (newEvent != null)
                    Scheduler.Add(newEvent);
            }


            //FUTURE:  background real-time animation goes in around here
        }

        private bool _inMultiKeyCommand = false;
        private Action<RLKeyPress> MultiKeyCommand = null;

        private void HandlePlayerCommands()
        {
            var key = InputQueue.Dequeue();
            if (_inMultiKeyCommand)
            {
                //  Drop, throw, wield, etc.
                MultiKeyCommand(key);
                return;
            }

            var direction = DirectionOfKey(key);
            if (direction != Direction.None)
            {
                MovePlayer(Player, direction);
            }
            else if (key.Key == RLKey.I)
            {
                Command_Inventory();
            }
            else if (key.Key == RLKey.H || key.Key == RLKey.Slash && key.Shift)
            {
                Command_Help();
            }
            else if (key.Key == RLKey.Comma)
            {
                Command_PickUp();
            }
            else if (key.Key == RLKey.D)
            {
                _inMultiKeyCommand = true;
                MultiKeyCommand = Command_Drop;
                Command_Drop_State = Command_Drop_States.Starting;
                MultiKeyCommand(key);
            }

            //TODO: all the other commands
        }

        private void Command_PickUp()
        {
            var topItem = Map.Items
                .Where(i => i.X == Player.X && i.Y == Player.Y)
                .LastOrDefault();

            if (topItem != null)
            {
                Map.Items.Remove(topItem);
                Player.Inventory.Add(topItem);
                Console.WriteLine($"Picked up {topItem.Name}");
            }
            else
            {
                Console.WriteLine("Nothing to pick up here.");
            }
        }

        private static void Command_Help()
        {
            Console.WriteLine("Help:");
            Console.WriteLine("Arrow keys to move");
            Console.WriteLine("i)nventory");
            Console.WriteLine("h)elp (or ?)");
            Console.WriteLine(",) Pick up object");
            Console.WriteLine("d)rop item from inventory");
        }

        private void Command_Inventory()
        {
            Console.WriteLine("Inventory:");
            if (Player.Inventory.Count == 0)
            {
                Console.WriteLine("empty.");
            }
            else
            {
                int asciiSlot = 97;
                foreach (var item in Player.Inventory)
                {
                    Console.WriteLine($"{(char)asciiSlot}) {item.Name}");
                    asciiSlot++;
                }
            }
        }

        private void Command_Drop(RLKeyPress key)
        {
            switch (Command_Drop_State)
            {
            case Command_Drop_States.Unknown:
                throw new Exception("Missed setup somewhere.");

            case Command_Drop_States.Starting:
                Console.Write("Drop: ");
                Console.Out.Flush();
                Command_Drop_State = Command_Drop_States.Expecting_Selection;
                break;

            case Command_Drop_States.Expecting_Selection:
                if (key.Key == RLKey.Escape)
                {
                    _inMultiKeyCommand = false;
                    MultiKeyCommand = null;
                    Command_Drop_State = Command_Drop_States.Unknown;
                    Console.WriteLine("nothing.");
                }
                else if (key.Key == RLKey.Slash && key.Shift)
                {
                    Command_Inventory();
                    Console.Write("Drop: ");
                    Console.Out.Flush();
                }
                else
                {
                    int inventorySlot = AlphaIndexOfKeyPress(key);
                    if (inventorySlot > -1)
                    {
                        if (Player.Inventory.Count() > inventorySlot)
                        {
                            var item = Player.Inventory[inventorySlot];
                            Player.Inventory.RemoveAt(inventorySlot);
                            item.MoveTo(Player.X, Player.Y);
                            Map.Items.Add(item);
                            Console.WriteLine(item.Name);

                            _inMultiKeyCommand = false;
                            MultiKeyCommand = null;
                            Command_Drop_State = Command_Drop_States.Unknown;
                        }
                        else
                        {
                            Console.WriteLine($"No item in slot '{key.Char.Value}'.");
                            Console.Write("Drop: ");
                            Console.Out.Flush();
                        }
                    }
                }
                break;

            default:
                throw new Exception("Command_Drop went to a weird place.");
            }
        }

        private int AlphaIndexOfKeyPress(RLKeyPress key)
        {
            if (!key.Char.HasValue) return -1;
            var asciiNum = (int)key.Char.Value;
            if (asciiNum < 97 || asciiNum > 123) return -1;
            return asciiNum - 97;
        }

        private Command_Drop_States Command_Drop_State;

        private enum Command_Drop_States
        {
            Unknown = 0,
            Starting,
            Expecting_Selection,
        }


        private void MovePlayer(IActor player, Direction direction)
        {
            var newX = player.X;
            var newY = player.Y;
            if (direction == Direction.Up) newY--;
            if (direction == Direction.Down) newY++;
            if (direction == Direction.Left) newX--;
            if (direction == Direction.Right) newX++;

            //  If we actually do move in that direction,
            //  we need to redraw, and the player will be busy for 12 ticks.
            if (Map.SetActorPosition(player, newX, newY))
            {
                _displayDirty = true;
                _readyForUserInput = false;
                Scheduler.Add(new ScheduleEntry(12, PlayerReadyForInput));
            }
        }

        private IScheduleEntry PlayerReadyForInput()
        {
            _readyForUserInput = true;
            return null;
        }

        private Direction DirectionOfKey(RLKeyPress keyPress)
        {
            return
                keyPress.Key == RLKey.Up ? Direction.Up :
                keyPress.Key == RLKey.Down ? Direction.Down :
                keyPress.Key == RLKey.Left ? Direction.Left :
                keyPress.Key == RLKey.Right ? Direction.Right :
                Direction.None;
        }
    }
}
