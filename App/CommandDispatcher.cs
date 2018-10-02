﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CopperBend.App.Model;
using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public enum DispatcherPhase
    {
        Unknown = 0,
        MessagesPending,
        PlayerReady,
        Schedule
    }

    public class CommandDispatcher
    {
        private bool InMultiKeyCommand
        {
            get => MultiKeyCommand != null;
        }
        private Action<RLKeyPress> MultiKeyCommand = null;
        public bool ReadyForUserInput = true;
        public bool WaitingToFinishMessages = false;

        private Queue<RLKeyPress> InputQueue;
        public Actor Player { get; private set; }
        public IAreaMap Map { get; private set; }
        public Scheduler Scheduler { get; private set; }
        public DispatcherPhase Phase { get; private set; }
        public Queue<string> MessageQueue { get; set; }

        public CommandDispatcher(Queue<RLKeyPress> inputQueue, Scheduler scheduler)
        {
            InputQueue = inputQueue;
            Scheduler = scheduler;
            MessageQueue = new Queue<string>();
        }

        public void Init(IAreaMap map, Actor player)
        {
            Map = map;
            Player = player;
            Phase = DispatcherPhase.MessagesPending;

            Message("I wake up.  Cold--frost on the ground, except where I was lying.");
            Message("Everything hurts when I stand up.");
            Message("The sky... says it's morning.  A small farmhouse to the east.");
            Message("Something real wrong with the ground to the west, and the north.");
        }

        public void Next()
        {
            switch (Phase)
            {
            //  When we have more messages to show
            case DispatcherPhase.MessagesPending:
                while (WaitingToFinishMessages && InputQueue.Any())
                {
                    HandlePendingMessages();
                }
                break;

            //  When the player is ready to act
            case DispatcherPhase.PlayerReady:
                while (ReadyForUserInput && InputQueue.Any())
                {
                    HandlePlayerCommands();
                }
                break;

            case DispatcherPhase.Schedule:
                var nextUp = Scheduler.GetNext();

                if (nextUp == null)
                    Debugger.Break();
                //  The scheduled event is called here
                var newEvent = nextUp.Action(nextUp, Map, Player);
                //  ...which may immediately schedule another event
                if (newEvent != null)
                    Scheduler.Add(newEvent);
                break;

            case DispatcherPhase.Unknown:
                throw new Exception("Dispatcher phase unknown, perhaps Init() was missed.");

            default:
                throw new Exception($"Dispatcher for phase [{Phase}] not written yet.");
            }

            //FUTURE:  background real-time animation goes in around here?
        }


        #region Messages
        private int ShownMessages = 0;

        public void Message(string newMessage)
        {
            MessageQueue.Enqueue(newMessage);
            ShowMessages();
        }

        public void ShowMessages()
        {
            while (!WaitingToFinishMessages && MessageQueue.Any())
            {
                var nextMessage = MessageQueue.Dequeue();
                Console.Out.WriteLine(nextMessage);
                ShownMessages++;
                if (ShownMessages >= 3)
                {
                    Console.Out.WriteLine("-- more --");
                    WaitingToFinishMessages = true;
                    Phase = DispatcherPhase.MessagesPending;
                }
            }
        }

        public void ClearMessagePanel()
        {
            //0.1
            ShownMessages = 0;
            WaitingToFinishMessages = false;
        }

        public void HandlePendingMessages()
        {
            //  We have exactly one reason to be here.
            if (Phase != DispatcherPhase.MessagesPending)
            {
                throw new Exception("Shouldn't try to handle pending messages when there are none.");
                //return;
            }

            while (WaitingToFinishMessages)
            {
                //  Advance to next space keypress, if any
                RLKeyPress key = null;
                while (InputQueue.Any() && key?.Key != RLKey.Space)
                {
                    key = InputQueue.Dequeue();
                }

                //  If we run out of keypresses before we find a space,
                // the rest of the messages remain pending
                if (key?.Key != RLKey.Space) return;

                //  Otherwise, show more messages
                ClearMessagePanel();
                ShowMessages();
            }

            //  If we reach this point, we cleared all messages
            Phase = ReadyForUserInput
                ? DispatcherPhase.PlayerReady
                : DispatcherPhase.Schedule;
        }
        #endregion

        public void HandlePlayerCommands()
        {
            var key = InputQueue.Dequeue();
            if (InMultiKeyCommand)
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
                enter_Apply(key);
            }
            else if (key.Key == RLKey.D)
            {
                enter_Drop(key);
            }
            else if (key.Key == RLKey.H || key.Key == RLKey.Slash && key.Shift)
            {
                Command_Help();
            }
            else if (key.Key == RLKey.I)
            {
                Command_Inventory();
            }
            else if (key.Key == RLKey.W)
            {
                enter_Wield(key);
            }
            else if (key.Key == RLKey.Comma)
            {
                Command_PickUp();
            }

            //TODO: all the other commands
        }

        #region Direction
        private void Command_Direction(IActor player, Direction direction)
        {
            var coord = newCoord(player, direction);

            IActor targetActor = Map.ActorAtCoord(coord);
            if (targetActor == null)
            {
                Command_DirectionMove(player, coord);
            }
            else
            {
                Command_DirectionAttack(targetActor, coord);
            }
        }
        private void Command_DirectionMove(IActor player, ICoord coord)
        {
            //  If we actually do move in that direction,
            //  we need to redraw, and the player will be busy for a while.
            if (Map.SetActorCoord(player, coord))
            {
                Map.DisplayDirty = true;
                if (player.X != coord.X && player.Y != coord.Y)
                    PlayerBusyFor(17);
                else
                    PlayerBusyFor(12);
            }
            else
            {
                ITile tile = Map[coord];
                if (tile.TerrainType == TerrainType.ClosedDoor)
                {
                    tile.OpenDoor();
                    Map.SetIsWalkable(tile, true);
                    Map.SetIsTransparent(tile, true);
                    Map.DisplayDirty = true;
                    Map.UpdatePlayerFieldOfView(player);
                    PlayerBusyFor(4);
                }
            }
        }
        private void Command_DirectionAttack(IActor targetActor, ICoord coord)
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
        #endregion

        #region Apply Item
        private void enter_Apply(RLKeyPress key)
        {
            MultiKeyCommand = Command_ApplyItem;
            Command_Apply_State = Command_Apply_States.Starting;
            MultiKeyCommand(key);
        }
        private void leave_Apply()
        {
            MultiKeyCommand = null;
            Command_Apply_State = Command_Apply_States.Unknown;
        }

        private IItem _usingItem;
        private void Command_ApplyItem(RLKeyPress key)
        {
            switch (Command_Apply_State)
            {
            case Command_Apply_States.Unknown:
                throw new Exception("Missed Apply setup somewhere.");

            case Command_Apply_States.Starting:
                //TODO: Gracefully handle player wielding nothing or non-tool
                if (Player.WieldedTool == null)
                {
                    Console.WriteLine("Not currently wielding a tool...todo.");
                    leave_Apply();
                    return;
                }
                _usingItem = Player.WieldedTool;
                Console.Write($"Use {_usingItem.Name} in direction (or ? to pick a different tool): ");
                Console.Out.Flush();
                Command_Apply_State = Command_Apply_States.Direction_or_ChangeTool;
                break;

            case Command_Apply_States.Direction_or_ChangeTool:
                var direction = DirectionOfKey(key);
                if (direction != Direction.None)
                {
                    Console.WriteLine(direction.ToString());
                    Console.Out.Flush();
                    Command_ApplyAction(direction);
                    leave_Apply();
                }
                else if (key.Key == RLKey.Escape)
                {
                    Console.WriteLine("cancelled.");
                    leave_Apply();
                }
                else if (key.Key == RLKey.Slash && key.Shift)
                {
                    Command_Inventory();
                    Console.WriteLine();
                    Command_Apply_State = Command_Apply_States.Select_new_Tool;
                }
                else
                {
                    //TODO: some 'not a supported choice' complaint?
                    //What's to be the standard?
                    //Nothing, for now.  It works well enough.
                }
                break;

            case Command_Apply_States.Select_new_Tool:
                var selectedIndex = AlphaIndexOfKeyPress(key);
                if (selectedIndex < 0 || selectedIndex > Player.Inventory.Count())
                {
                    Console.Write($"The key [{key.Char}] does not match an inventory item.");
                }
                else
                {
                    var item = Player.Inventory.ElementAt(selectedIndex);
                    if (item.IsUsable)
                    {
                        _usingItem = item;
                    }
                    else
                    {
                        Console.Write($"The item [{item.Name}] is not a usable tool.");
                    }
                }
                Console.WriteLine();
                break;

            default:
                throw new Exception($"Command_ApplyItem not ready for state {Command_Apply_State}.");
            }
        }

        private void Command_ApplyAction(Direction direction)
        {
            //  Possible?  (Can't hoe rock, may have custom message)
            //  Successful?  (Skill/difficulty check)
            //  Effects.  If it did nothing, we wouldn't be here.
            //  Output.
            var targetCoord = newCoord(Player, direction);
            var tile = Map[targetCoord];
            switch (_usingItem.Name)
            {
            case "hoe":
                if (tile.IsTillable)
                {
                    tile.Till();
                    Map.DisplayDirty = true;
                    PlayerBusyFor(15);
                }
                else
                {
                    var cell = Map.GetCell(targetCoord.X, targetCoord.Y);
                    Console.Out.WriteLine($"Cannot hoe {tile.TerrainType}.");
                }
                break;

            case "seed":
                if (tile.IsTilled)
                {
                    tile.Till();
                    Map.DisplayDirty = true;
                    PlayerBusyFor(15);
                }
                else
                {
                    var cell = Map.GetCell(targetCoord.X, targetCoord.Y);
                    Console.Out.WriteLine($"Cannot hoe {tile.TerrainType}.");
                }
                break;

            default:
                throw new Exception($"Using tool [{_usingItem.Name}] not yet written.");
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
        #endregion

        #region Drop

        private void enter_Drop(RLKeyPress key)
        {
            MultiKeyCommand = Command_Drop;
            Command_Drop_State = Command_Drop_States.Starting;
            MultiKeyCommand(key);
        }
        private void leave_Drop()
        {
            MultiKeyCommand = null;
            Command_Drop_State = Command_Drop_States.Unknown;
        }

        private void Command_Drop(RLKeyPress key)
        {
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
                        if (Player.WieldedTool == item)
                        {
                            Console.WriteLine($"Note:  No longer wielding the {item.Name}.");
                            Player.WieldedTool = null;
                        }

                        PlayerBusyFor(1);

                        leave_Drop();
                    }
                    else
                    {
                        Console.WriteLine($"No item labelled '{key.Char.Value}'.");
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
        #endregion

        private void Command_Help()
        {
            Console.WriteLine("Help:");
            Console.WriteLine("Arrow or numpad keys to move and attack");
            Console.WriteLine("a)pply wielded tool");
            Console.WriteLine("d)rop an item");
            Console.WriteLine("h)elp (or ?) shows this message");
            Console.WriteLine("i)nventory");
            Console.WriteLine("w)ield a tool");
            Console.WriteLine(",) Pick up object");
        }

        private void Command_Inventory()
        {
            var watcher = new ItemWatcher();

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
                    var description = watcher.Describe(item);
                    Console.WriteLine($"{(char)asciiSlot})  {description}");
                    asciiSlot++;
                }
            }
        }

        #region Wield
        private void enter_Wield(RLKeyPress key)
        {
            MultiKeyCommand = Command_Wield;
            Command_Wield_State = Command_Wield_States.Starting;
            MultiKeyCommand(key);
        }
        private void leave_Wield()
        {
            MultiKeyCommand = null;
            Command_Wield_State = Command_Wield_States.Unknown;
        }

        private void Command_Wield(RLKeyPress key)
        {
            switch (Command_Wield_State)
            {
            case Command_Wield_States.Unknown:
                throw new Exception("Missed Wield setup somewhere.");

            case Command_Wield_States.Starting:
                Console.Write("Wield ('?' to show inventory, '.' to empty hands): ");
                Console.Out.Flush();
                Command_Wield_State = Command_Wield_States.Expecting_Selection;
                break;

            case Command_Wield_States.Expecting_Selection:
                if (key.Key == RLKey.Escape)
                {
                    var name = Player.WieldedTool == null
                        ? "bare hands"
                        : Player.WieldedTool.Name;
                    Console.WriteLine($"unchanged, {name}.");
                    leave_Wield();
                }
                else if (key.Key == RLKey.Period)
                {
                    Console.WriteLine("bare hands");
                    Player.WieldedTool = null;
                    leave_Wield();
                }
                else if (key.Key == RLKey.Slash && key.Shift)
                {
                    Command_Inventory();
                    Console.Write("Wield: ");
                    Console.Out.Flush();
                }
                else
                {
                    int inventorySlot = AlphaIndexOfKeyPress(key);
                    if (inventorySlot == -1) break;

                    if (Player.Inventory.Count() > inventorySlot)
                    {
                        // we can wield even silly stuff, until it's a problem.
                        var item = Player.Inventory.ElementAt(inventorySlot);

                        Player.WieldedTool = item;
                        Console.WriteLine(item.Name);
                        PlayerBusyFor(4);

                        leave_Wield();
                    }
                    else
                    {
                        Console.WriteLine($"No item in slot '{key.Char.Value}'.");
                        Console.Write("Wield: ");
                        Console.Out.Flush();
                    }
                }
                break;

            default:
                throw new Exception($"Command_Wield not ready for state {Command_Wield_State}.");
            }
        }
        private enum Command_Wield_States
        {
            Unknown = 0,
            Starting,
            Expecting_Selection,
        }
        private Command_Wield_States Command_Wield_State;
        #endregion

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
            Phase = DispatcherPhase.Schedule;
        }

        private ScheduleEntry PlayerReadyForInput(ScheduleEntry entry, IAreaMap map, IActor player)
        {
            ReadyForUserInput = true;
            Phase = DispatcherPhase.PlayerReady;
            return null;
        }

        private ICoord newCoord(ICoord start, Direction direction)
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

            return new Coord(newX, newY);
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