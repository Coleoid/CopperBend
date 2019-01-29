using log4net;
using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CopperBend.App
{
    public class InputCommandSource : ICommandSource
    {
        private Queue<RLKeyPress> InQ { get; set; }
        private IActor Actor { get; set; }
        private Describer Describer { get; set; }
        private IGameWindow Window { get; set; }
        private readonly ILog log;


        public InputCommandSource(Queue<RLKeyPress> inQ, Describer describer, IGameWindow window)
        {
            InQ = inQ;
            Describer = describer;
            Window = window;
            log = LogManager.GetLogger("CB.InputMapper");
        }
        

        public bool InMultiStepCommand => NextStep != null;
        private Func<RLKeyPress, Command> NextStep = null;
        private bool QueueIsEmpty => InQ.Count == 0;

        private readonly Command CommandNone = new Command(CmdAction.None, CmdDirection.None);
        private readonly Command CommandUnknown = new Command(CmdAction.Unknown, CmdDirection.None);
        private const int lowercase_a = 97;
        private const int lowercase_z = 123;

        public void SetActor(IActor actor)
        {
            Actor = actor;
        }
        public Command GetCommand()
        {
            if (QueueIsEmpty) return CommandNone;

            var press = InQ.Dequeue();
            if (InMultiStepCommand)  //  Drop, throw, wield, etc.
            {
                return NextStep(press);
            }

            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                return new Command(CmdAction.Move, dir);
            }

            switch (press.Key)
            {
            case RLKey.C: return Consume();
            case RLKey.D: return Drop();
            case RLKey.H: return Help();
            case RLKey.I: return Inventory();
            case RLKey.U: return Use();
            case RLKey.W: return Wield();
            case RLKey.Comma: return PickUp();
            default: return CommandUnknown;
            }
        }

        public Command Consume()
        {
            return NextWhenReady( Consume_main, "Consume (inventory letter or ? to show inventory): ");
        }
        public Command Consume_main(RLKeyPress press)
        {
            if (press.Key == RLKey.Slash && press.Shift)
            {
                ShowInventory(i => i.IsConsumable);
                return CommandNone;
            }

            var item = ItemInInventoryLocation(press);
            if (item != null)
            {
                NextStep = null;
                return new Command(CmdAction.Consume, CmdDirection.None, item);
            }

            return CommandNone;
        }

        public Command Drop()
        {
            return NextWhenReady( Drop_main, "Drop (inventory letter or ? to show inventory): ");
        }
        public Command Drop_main(RLKeyPress press)
        {
            if (press.Key == RLKey.Slash && press.Shift)
            {
                ShowInventory(i => true);
                return CommandNone;
            }

            var item = ItemInInventoryLocation(press);
            if (item != null)
            {
                NextStep = null;
                return new Command(CmdAction.Drop, CmdDirection.None, item);
            }

            return CommandNone;
        }
        //HMMM:  Consume_main and Drop_main differ only in CmdAction and inventory filter

        public Command Help() {
            WriteLine("Help:");
            WriteLine("Arrow or numpad keys to move and attack");
            WriteLine("a)pply wielded tool");
            WriteLine("d)rop an item");
            WriteLine("h)elp (or ?) shows this message");
            WriteLine("i)nventory");
            WriteLine("w)ield a tool");
            WriteLine(",) Pick up object");
            return CommandNone;
        }

        public Command Inventory()
        {
            ShowInventory(i => true);
            return CommandNone;
        }

        #region Use
        private IItem PriorUsedItem = null;
        private IItem ThisUsedItem = null;
        public Command Use()
        {
            ThisUsedItem = ThisUsedItem ?? PriorUsedItem ?? Actor.WieldedTool;
            if (ThisUsedItem == null)
            {
                ShowInventory(i => i.IsUsable);
                return NextWhenReady(Use_Pick_Item, "Use item: ");
            }

            return NextWhenReady(Use_Has_Item, 
                $"Direction to use the {Describer.Describe(ThisUsedItem)}, or [a-z?] to choose item: ");
        }
        public Command Use_Pick_Item(RLKeyPress press)
        {
            if (press.Key == RLKey.Escape)
            {
                WriteLine("cancelled.");
                NextStep = null;
                return CommandNone;
            }

            var selectedIndex = AlphaIndexOfKeyPress(press);
            if (selectedIndex < 0 || Actor.Inventory.Count() <= selectedIndex)
            {
                WriteLine($"The key [{PressRep(press)}] does not match an inventory item.  Pick another.");
                return CommandNone;
            }

            var item = Actor.Inventory.ElementAt(selectedIndex);
            if (!item.IsUsable)
            {
                WriteLine($"The {Describer.Describe(item)} is not a usable item.  Pick another.");
                return CommandNone;
            }

            ThisUsedItem = item;

            return NextWhenReady( Use_Has_Item, 
                $"Direction to use the {Describer.Describe(ThisUsedItem)}, or [a-z?] to choose item: ");
        }
        private Command Use_Has_Item(RLKeyPress press)
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
                ShowInventory(i => i.IsUsable);
            }

            if ('a' <= press.Char && press.Char <= 'z')
            {
                return Use_Pick_Item(press);
            }

            if (press.Key == RLKey.Escape)
            {
                WriteLine("cancelled.");
                NextStep = null;
            }

            WriteLine($"The key [{PressRep(press)}] does not match an inventory item or a direction.  Pick another.");
            return CommandNone;

            //  Some of this needs to end up in Action system
            //    var targetPoint = PointInDirection(Player.Point, direction);
            //    WriteLine(direction.ToString());
            //    _usingItem.ApplyTo(Map[targetPoint], this, direction);  // the magic
        }
#endregion

        public Command Wield() { return CommandNone; }

        public Command PickUp()
        {
            var topItem = Actor.ReachableItems()
                .LastOrDefault();

            if (topItem == null)
            {
                WriteLine("Nothing to pick up here.");
                return CommandNone;
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

        private Command NextWhenReady(Func<RLKeyPress, Command> nextStep, string prompt)
        {
            NextStep = nextStep;
            if (QueueIsEmpty)
            {
                Prompt(prompt);
                return CommandNone;
            }
            return NextStep(InQ.Dequeue());
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

        private IItem ItemInInventoryLocation(RLKeyPress press)
        {
            int inventorySlot = AlphaIndexOfKeyPress(press);
            if (-1 < inventorySlot && inventorySlot < Actor.Inventory.Count())
            {
                return Actor.Inventory.ElementAt(inventorySlot);
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

        public void ShowInventory(Func<IItem, bool> filter = null)
        {
            Window.ShowInventory(Actor.Inventory, filter);
        }

        private void WriteLine(string line)
        {
            Window.WriteLine(line);
        }
        #endregion
    }
}
