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

        private readonly Command CommandIncomplete = new Command(CmdAction.Incomplete, CmdDirection.None);
        private const int lowercase_a = 97;
        private const int lowercase_z = 123;

        public void GiveCommand(IActor actor)
        {
            bool deliverCommandFromInput()
            {
                var cmd = GetCommand(actor);
                if (cmd.Action == CmdAction.Incomplete) return false;

                var actionWasTaken = Controls.CommandActor(actor, cmd);
                if (actionWasTaken) NextStep = null;

                return actionWasTaken;
            }

            var commandGiven = deliverCommandFromInput();
            if (!commandGiven)
            {
                Bus.EnterMode(EngineMode.InputBound, deliverCommandFromInput);
            }
        }

        internal Command GetCommand(IActor actor)
        {
            if (QueueIsEmpty) return CommandIncomplete;
            var press = InQ.Dequeue();

            if (InMultiStepCommand)  //  Most of them
            {
                return NextStep(press, actor);
            }

            //  Going somewhere?
            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                return new Command(CmdAction.Direction, dir);
            }

            switch (press.Key)
            {
            case RLKey.C: return Consume(actor);
            case RLKey.D: return Drop(actor);
            case RLKey.H: return Help();
            case RLKey.I: return Inventory(actor);
            case RLKey.U: return Use(actor);
            case RLKey.W: return Wield(actor);
            case RLKey.Comma: return PickUp(actor);

            default:
                WriteLine($"Command [{press.Char}] is unknown.");
                return CommandIncomplete;
            }
        }

        public Command Consume(IActor actor)
        {
            var inv_consumables = actor.Inventory.Where(i => i.IsConsumable).ToList();
            //var reach_consumables = actor.ReachableItems().Where(i => i.IsConsumable).ToList();
            if (inv_consumables.Count() == 0) /*+ reach_consumables.Count()*/
            {
                WriteLine("Nothing to eat or drink.");
                return CommandIncomplete;
            }
            return FFwdOrPrompt(Consume_main, "Do_Consume (inventory letter or ? to show inventory): ", actor);
        }
        public Command Consume_main(RLKeyPress press, IActor actor)
        {
            if (press.Key == RLKey.Escape)
            {
                WriteLine("Do_Consume cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

            if (press.Key == RLKey.Slash && press.Shift)
            {
                ShowInventory(actor, i => i.IsConsumable);
                return CommandIncomplete;
            }

            var item = ItemInInventoryLocation(press, actor);
            if (item != null)
            {
                if (item.IsConsumable)
                {
                    NextStep = null;
                    return new Command(CmdAction.Consume, CmdDirection.None, item);
                }
                else
                {
                    WriteLine($"I can't eat or drink {Describer.Describe(item, DescMods.IndefiniteArticle)}.");
                }
            }
            else
            {
                WriteLine($"Nothing in inventory slot {press.Char}.");
            }
            return CommandIncomplete;
        }

        public Command Drop(IActor actor)
        {
            if (actor.Inventory.Count() == 0)
            {
                WriteLine("Nothing to drop.");
                return CommandIncomplete;
            }
            return FFwdOrPrompt(Drop_main, "Drop (inventory letter or ? to show inventory): ", actor);
        }
        public Command Drop_main(RLKeyPress press, IActor actor)
        {
            if (press.Key == RLKey.Escape)
            {
                WriteLine("Drop cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

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
            else
            {
                WriteLine($"Nothing in inventory slot {press.Char}.");
            }

            return CommandIncomplete;
        }
        //HMMM:  Consume_main and Drop_main differ only in CmdAction and inventory filter

        public Command Help()
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

        public Command Wield(IActor actor)
        {
            var wieldables = actor.Inventory.ToList();
            if (wieldables.Count() == 0)
            {
                WriteLine("Nothing to wield.");
                return CommandIncomplete;
            }
            return FFwdOrPrompt(Wield_main, "Wield (inventory letter or ? to show inventory): ", actor);
        }

        public Command Wield_main(RLKeyPress press, IActor actor)
        {
            if (press.Key == RLKey.Escape)
            {
                WriteLine("Wield cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

            if (press.Key == RLKey.Slash && press.Shift)
            {
                ShowInventory(actor, i => true);
                return CommandIncomplete;
            }

            var item = ItemInInventoryLocation(press, actor);
            if (item != null)
            {
                NextStep = null;
                return new Command(CmdAction.Wield, CmdDirection.None, item);
            }
            else
            {
                WriteLine($"Nothing in inventory slot {press.Char}.");
            }
            return CommandIncomplete;
        }

        public Command PickUp(IActor actor) //0.2
        {
            //  Right now, simply grabs the topmost
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

        /// <summary> If more input is queued, the prompt will not be sent /// 
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
