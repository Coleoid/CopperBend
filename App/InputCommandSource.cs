﻿using log4net;
using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class InputCommandSource : ICommandSource
    {
        private Queue<RLKeyPress> InQ { get; set; }
        private Describer Describer { get; set; }
        private IGameWindow Window { get; set; }
        private IControlPanel Controls { get; set; }
        private readonly EventBus Bus;
        private readonly ILog log;

        public InputCommandSource(Queue<RLKeyPress> inQ, Describer describer, IGameWindow window, EventBus bus, IControlPanel controls)
        {
            InQ = inQ;
            Describer = describer;
            Window = window;
            Bus = bus;
            Controls = controls;
            log = LogManager.GetLogger("CB.InputCommandSource");
        }
        

        public bool InMultiStepCommand => NextStep != null;
        private Func<RLKeyPress, IActor, Command> NextStep = null;
        private bool QueueIsEmpty => InQ.Count == 0;

        private readonly Command CommandIncomplete = new Command(CmdAction.None, CmdDirection.None);
        private readonly Command CommandUnknown = new Command(CmdAction.Unknown, CmdDirection.None);
        private const int lowercase_a = 97;
        private const int lowercase_z = 123;

        public void GiveCommand(IActor actor)
        {
            bool consume_input_until_command_given()
            {
                var cmd = GetCommand(actor);
                bool finishedTurn = false;
                bool gotCommand = cmd.Action != CmdAction.None;
                if (gotCommand)
                {
                    //finishedTurn = actor.Command(cmd);
                    //finishedTurn = ((CommandDispatcher) Controls).HandlePlayerCommands(A);
                    if (finishedTurn)
                        NextStep = null;
                }

                return gotCommand && finishedTurn;
            }

            var gaveCommand = consume_input_until_command_given();
            if (!gaveCommand)
            {
                Bus.EnterMode(EngineMode.InputBound, consume_input_until_command_given);
            }
        }

        internal Command GetCommand(IActor actor)
        {
            if (QueueIsEmpty) return CommandIncomplete;

            var press = InQ.Dequeue();
            if (InMultiStepCommand)  //  Drop, throw, wield, etc.
            {
                return NextStep(press, actor);
            }

            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                return new Command(CmdAction.Move, dir);
            }

            switch (press.Key)
            {
            case RLKey.C: return Consume(actor);
            case RLKey.D: return Drop(actor);
            case RLKey.H: return Help(actor);
            case RLKey.I: return Inventory(actor);
            case RLKey.U: return Use(actor);
            case RLKey.W: return Wield(actor);
            case RLKey.Comma: return PickUp(actor);
            default: return CommandUnknown;
            }
        }

        public Command Consume(IActor actor)
        {
            return FFwdOrPrompt( Consume_main, "Consume (inventory letter or ? to show inventory): ", actor);
        }
        public Command Consume_main(RLKeyPress press, IActor actor)
        {
            if (press.Key == RLKey.Slash && press.Shift)
            {
                ShowInventory(actor, i => i.IsConsumable);
                return CommandIncomplete;
            }

            var item = ItemInInventoryLocation(press, actor);
            if (item != null)
            {
                NextStep = null;
                return new Command(CmdAction.Consume, CmdDirection.None, item);
            }

            return CommandIncomplete;
        }

        public Command Drop(IActor actor)
        {
            return FFwdOrPrompt( Drop_main, "Drop (inventory letter or ? to show inventory): ", actor);
        }
        public Command Drop_main(RLKeyPress press, IActor actor)
        {
            if (press.Key == RLKey.Slash && press.Shift)
            {
                ShowInventory(actor, i => true);
                return CommandIncomplete;
            }

            var item = ItemInInventoryLocation(press, actor);
            if (item != null)
            {
                NextStep = null;
                return new Command(CmdAction.Drop, CmdDirection.None, item);
            }

            return CommandIncomplete;
        }
        //HMMM:  Consume_main and Drop_main differ only in CmdAction and inventory filter

        public Command Help(IActor actor)
        {
            WriteLine("Help:");
            WriteLine("Arrow or numpad keys to move and attack");
            WriteLine("a)pply wielded tool");
            WriteLine("d)rop an item");
            WriteLine("h)elp (or ?) shows this message");
            WriteLine("i)nventory");
            WriteLine("w)ield a tool");
            WriteLine(",) Pick up object");
            return CommandIncomplete;
        }

        public Command Inventory(IActor actor)
        {
            ShowInventory(actor, i => true);
            return CommandIncomplete;
        }

        #region Use
        private IItem PriorUsedItem = null;
        private IItem ThisUsedItem = null;
        public Command Use(IActor actor)
        {
            ThisUsedItem = ThisUsedItem ?? PriorUsedItem ?? actor.WieldedTool;
            if (ThisUsedItem == null)
            {
                ShowInventory(actor, i => i.IsUsable);
                return FFwdOrPrompt(Use_Pick_Item, "Use item: ", actor);
            }

            return FFwdOrPrompt(Use_Has_Item, 
                $"Direction to use the {Describer.Describe(ThisUsedItem)}, or [a-z?] to choose item: ", actor);
        }
        public Command Use_Pick_Item(RLKeyPress press, IActor actor)
        {
            if (press.Key == RLKey.Escape)
            {
                WriteLine("cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

            var selectedIndex = AlphaIndexOfKeyPress(press);
            if (selectedIndex < 0 || actor.Inventory.Count() <= selectedIndex)
            {
                WriteLine($"The key [{PressRep(press)}] does not match an inventory item.  Pick another.");
                return CommandIncomplete;
            }

            var item = actor.Inventory.ElementAt(selectedIndex);
            if (!item.IsUsable)
            {
                WriteLine($"The {Describer.Describe(item)} is not a usable item.  Pick another.");
                return CommandIncomplete;
            }

            ThisUsedItem = item;

            return FFwdOrPrompt( Use_Has_Item, 
                $"Direction to use the {Describer.Describe(ThisUsedItem)}, or [a-z?] to choose item: ", actor);
        }
        private Command Use_Has_Item(RLKeyPress press, IActor actor)
        {
            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                NextStep = null;
                PriorUsedItem = ThisUsedItem;
                ThisUsedItem = null;
                return new Command(CmdAction.Use, dir, PriorUsedItem);
            }

            if (press.Key == RLKey.Slash && press.Shift)
            {
                ShowInventory(actor, i => i.IsUsable);
            }

            if ('a' <= press.Char && press.Char <= 'z')
            {
                return Use_Pick_Item(press, actor);
            }

            if (press.Key == RLKey.Escape)
            {
                WriteLine("cancelled.");
                NextStep = null;
            }

            WriteLine($"The key [{PressRep(press)}] does not match an inventory item or a direction.  Pick another.");
            return CommandIncomplete;

            //  Some of this needs to end up in Action system
            //    var targetPoint = PointInDirection(Player.Point, direction);
            //    WriteLine(direction.ToString());
            //    _usingItem.ApplyTo(Map[targetPoint], this, direction);  // the magic
        }
#endregion

        public Command Wield(IActor actor) { return CommandIncomplete; }

        public Command PickUp(IActor actor)
        {
            var topItem = actor.ReachableItems()
                .LastOrDefault();

            if (topItem == null)
            {
                WriteLine("Nothing to pick up here.");
                return CommandIncomplete;
            }
            return new Command(CmdAction.PickUp, CmdDirection.None, topItem);
        }

        #region Utility
        private static string PressRep(RLKeyPress press)
        {
            string rep = press.Char.ToString();
            if (string.IsNullOrEmpty(rep)) rep = press.Key.ToString();
            return rep;
        }

        public CmdDirection DirectionOf(RLKeyPress press)
        {
            switch (press.Key)
            {
            case RLKey.Left: return CmdDirection.West;
            case RLKey.Right: return CmdDirection.East;
            case RLKey.Up: return CmdDirection.North;
            case RLKey.Down: return CmdDirection.South;
            case RLKey.Keypad1: return CmdDirection.Southwest;
            case RLKey.Keypad2: return CmdDirection.South;
            case RLKey.Keypad3: return CmdDirection.Southeast;
            case RLKey.Keypad4: return CmdDirection.West;
            case RLKey.Keypad6: return CmdDirection.East;
            case RLKey.Keypad7: return CmdDirection.Northwest;
            case RLKey.Keypad8: return CmdDirection.North;
            case RLKey.Keypad9: return CmdDirection.Northeast;
            default: return CmdDirection.None;
            }
        }

        private IItem ItemInInventoryLocation(RLKeyPress press, IActor actor)
        {
            int inventorySlot = AlphaIndexOfKeyPress(press);
            if (-1 < inventorySlot && inventorySlot < actor.Inventory.Count())
            {
                return actor.Inventory.ElementAt(inventorySlot);
            }
            return null;
        }

        private static int AlphaIndexOfKeyPress(RLKeyPress press)
        {
            if (!press.Char.HasValue) return -1;
            var asciiNum = (int)press.Char.Value;
            if (asciiNum < lowercase_a || lowercase_z < asciiNum) return -1;
            return asciiNum - lowercase_a;
        }

        public void Prompt(string text)
        {
            log.InfoFormat("Prompt:  [{0}]", text);
            Window.Prompt(text);
        }

        private Command FFwdOrPrompt(Func<RLKeyPress, IActor, Command> nextStep, string prompt, IActor actor)
        {
            NextStep = nextStep;
            if (QueueIsEmpty)
            {
                Prompt(prompt);
                return CommandIncomplete;
            }
            return NextStep(InQ.Dequeue(), actor);
        }

        public void ShowInventory(IActor actor, Func<IItem, bool> filter = null)
        {
            Window.ShowInventory(actor.Inventory, filter);
        }

        private void WriteLine(string line)
        {
            Window.WriteLine(line);
        }
        #endregion
    }
}
