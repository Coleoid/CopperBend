using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.App.Model;
using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public class CommandDispatcher
    {
        private bool _inMultiKeyCommand = false;
        private Action<RLKeyPress> MultiKeyCommand = null;
        public bool ReadyForUserInput = true;


        private Queue<RLKeyPress> InputQueue;
        public Actor Player { get; private set; }
        public IAreaMap Map { get; private set; }
        public Scheduler Scheduler { get; private set; }

        public CommandDispatcher(Queue<RLKeyPress> inputQueue, Scheduler scheduler)
        {
            InputQueue = inputQueue;
            Scheduler = scheduler;
        }

        internal void Init(IAreaMap map, Actor player)
        {
            Map = map;
            Player = player;
        }

        public void HandlePlayerCommands()
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
                Command_Direction(Player, direction);
            }
            else if (key.Key == RLKey.A)
            {
                _inMultiKeyCommand = true;
                MultiKeyCommand = Command_ApplyTool;
                Command_Apply_State = Command_Apply_States.Direction_or_ChangeTool;
                MultiKeyCommand(key);
            }
            else if (key.Key == RLKey.D)
            {
                _inMultiKeyCommand = true;
                MultiKeyCommand = Command_Drop;
                Command_Drop_State = Command_Drop_States.Starting;
                MultiKeyCommand(key);
            }
            else if (key.Key == RLKey.H || key.Key == RLKey.Slash && key.Shift)
            {
                Command_Help();
            }
            else if (key.Key == RLKey.I)
            {
                Command_Inventory();
            }
            else if (key.Key == RLKey.Comma)
            {
                Command_PickUp();
            }

            //TODO: all the other commands
        }


        private void Command_Direction(IActor player, Direction direction)
        {
            var (newX, newY) = newCoord(player, direction);

            IActor targetActor = Map.ActorAtLocation(newX, newY);
            if (targetActor == null)
            {
                Command_DirectionMove(player, newX, newY);
            }
            else
            {
                Command_DirectionAttack(targetActor, newX, newY);
            }
        }

        private void Command_DirectionMove(IActor player, int newX, int newY)
        {
            //  If we actually do move in that direction,
            //  we need to redraw, and the player will be busy for a while.
            if (Map.SetActorPosition(player, newX, newY))
            {
                Map.DisplayDirty = true;
                if (player.X != newX && player.Y != newY)
                    PlayerBusyFor(17);
                else
                    PlayerBusyFor(12);
            }
        }

        private void Command_DirectionAttack(IActor targetActor, int newX, int newY)
        {
            //0.1
            targetActor.Damage(2);
            Console.WriteLine("You hit the thingy for 2 points!");
            if (targetActor.Health < 1)
            {
                Console.WriteLine($"Blargh...  The {targetActor.Name} dies.");
                Map.Actors.Remove(targetActor);
                Map.SetIsWalkable(targetActor, true);
                Map.DisplayDirty = true;

                Scheduler.RemoveActor(targetActor);
            }

            PlayerBusyFor(12);
        }


        private void Command_ApplyTool(RLKeyPress key)
        {
            Action leave_Apply = () =>
            {
                _inMultiKeyCommand = false;
                MultiKeyCommand = null;
                Command_Apply_State = Command_Apply_States.Unknown;
            };

            switch (Command_Apply_State)
            {
            case Command_Apply_States.Unknown:
                throw new Exception("Missed Apply setup somewhere.");

            case Command_Apply_States.Starting:
                //TODO: pick the wielded tool as default
                Console.Write("Use hoe in direction (or ? to pick a different tool): ");
                Console.Out.Flush();
                Command_Apply_State = Command_Apply_States.Direction_or_ChangeTool;
                break;

            case Command_Apply_States.Direction_or_ChangeTool:
                var direction = DirectionOfKey(key);
                if (direction != Direction.None)
                {
                    //TODO: Invoke usage of that tool.
                    //  Possible?  (Can't hoe rock, may have custom message)
                    //  Successful?  (Skill/difficulty check)
                    //  Effects.  If it did nothing, we wouldn't be here.
                    //  Output.
                    leave_Apply();
                }
                else if (key.Key == RLKey.Escape)
                {
                    Console.WriteLine("cancelled.");
                    leave_Apply();
                }
                else if (key.Key == RLKey.Slash && key.Shift)
                {
                    //TODO: show inventory
                    //TODO: if key is usable tool, select it, message
                }
                else
                {
                    //TODO: some complaint?  What's to be the standard?
                }
                break;

            default:
                throw new Exception("Command_ApplyTool went to a weird place.");
            }
        }
        private enum Command_Apply_States
        {
            Unknown = 0,
            Starting,
            Direction_or_ChangeTool,
            Select_new_Tool,
        }
        private Command_Apply_States Command_Apply_State;


        private void Command_Drop(RLKeyPress key)
        {
            Action leave_Drop = () =>
            {
                _inMultiKeyCommand = false;
                MultiKeyCommand = null;
                Command_Drop_State = Command_Drop_States.Unknown;
            };

            switch (Command_Drop_State)
            {
            case Command_Drop_States.Unknown:
                throw new Exception("Missed Drop setup somewhere.");

            case Command_Drop_States.Starting:
                Console.Write("Drop: ");
                Console.Out.Flush();
                Command_Drop_State = Command_Drop_States.Expecting_Selection;
                break;

            case Command_Drop_States.Expecting_Selection:
                if (key.Key == RLKey.Escape)
                {
                    Console.WriteLine("nothing.");
                    leave_Drop();
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
                    if (inventorySlot == -1) break;

                    if (Player.Inventory.Count() > inventorySlot)
                    {
                        IItem item = Player.RemoveFromInventory(inventorySlot);
                        item.MoveTo(Player.X, Player.Y);
                        Map.Items.Add(item);
                        Console.WriteLine(item.Name);
                        PlayerBusyFor(1);

                        leave_Drop();
                    }
                    else
                    {
                        Console.WriteLine($"No item in slot '{key.Char.Value}'.");
                        Console.Write("Drop: ");
                        Console.Out.Flush();
                    }
                }
                break;

            default:
                throw new Exception("Command_Drop went to a weird place.");
            }
        }
        private enum Command_Drop_States
        {
            Unknown = 0,
            Starting,
            Expecting_Selection,
        }
        private Command_Drop_States Command_Drop_State;


        private void Command_Help()
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
            if (Player.Inventory.Count() == 0)
            {
                Console.WriteLine("empty.");
            }
            else
            {
                int asciiSlot = 97;
                foreach (var item in Player.Inventory)
                {
                    var text = item.Quantity > 1
                        ? $"{item.Quantity} {item.Name}s"
                        : $"a {item.Name}";
                    Console.WriteLine($"{(char)asciiSlot})  {text}");
                    asciiSlot++;
                }
            }
        }


        private void Command_PickUp()
        {
            var topItem = Map.Items
                .Where(i => i.X == Player.X && i.Y == Player.Y)
                .LastOrDefault();

            if (topItem == null)
            {
                Console.WriteLine("Nothing to pick up here.");
                return;
            }

            Map.Items.Remove(topItem);
            Player.AddToInventory(topItem);
            Console.WriteLine($"Picked up {topItem.Name}");
            PlayerBusyFor(2);
        }


        private int AlphaIndexOfKeyPress(RLKeyPress key)
        {
            if (!key.Char.HasValue) return -1;
            var asciiNum = (int)key.Char.Value;
            if (asciiNum < 97 || asciiNum > 123) return -1;
            return asciiNum - 97;
        }

        private void PlayerBusyFor(int ticks)
        {
            ReadyForUserInput = false;
            Scheduler.Add(new ScheduleEntry(ticks, PlayerReadyForInput));
        }

        private ScheduleEntry PlayerReadyForInput(ScheduleEntry entry, IAreaMap map, IActor player)
        {
            ReadyForUserInput = true;
            return null;
        }

        private (int, int) newCoord(ICoord start, Direction direction)
        {
            int newX = start.X;
            int newY = start.Y;

            if (direction == Direction.Up
                || direction == Direction.UpLeft
                || direction == Direction.UpRight)
            {
                newY--;
            }

            if (direction == Direction.Down
                || direction == Direction.DownLeft
                || direction == Direction.DownRight)
            {
                newY++;
            }

            if (direction == Direction.Left
                || direction == Direction.UpLeft
                || direction == Direction.DownLeft)
            {
                newX--;
            }

            if (direction == Direction.Right
                || direction == Direction.UpRight
                || direction == Direction.DownRight)
            {
                newX++;
            }

            return (newX, newY);
        }

        private Direction DirectionOfKey(RLKeyPress keyPress)
        {
            return
                keyPress.Key == RLKey.Up ? Direction.Up :
                keyPress.Key == RLKey.Down ? Direction.Down :
                keyPress.Key == RLKey.Left ? Direction.Left :
                keyPress.Key == RLKey.Right ? Direction.Right :

                keyPress.Key == RLKey.Keypad1 ? Direction.DownLeft :
                keyPress.Key == RLKey.Keypad2 ? Direction.Down :
                keyPress.Key == RLKey.Keypad3 ? Direction.DownRight :
                keyPress.Key == RLKey.Keypad4 ? Direction.Left :
                keyPress.Key == RLKey.Keypad6 ? Direction.Right :
                keyPress.Key == RLKey.Keypad7 ? Direction.UpLeft :
                keyPress.Key == RLKey.Keypad8 ? Direction.Up :
                keyPress.Key == RLKey.Keypad9 ? Direction.UpRight :
                Direction.None;
        }
    }
}
