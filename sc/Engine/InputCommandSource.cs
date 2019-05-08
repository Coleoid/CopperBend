using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.Contract;
using Microsoft.Xna.Framework.Input;
using SadConsole.Input;
using SadConsole;

namespace CopperBend.Engine
{
    public class InputCommandSource : ICommandSource
    {
        private Queue<AsciiKey> InQ { get; set; }
        private Describer Describer { get; set; }
        private Window Window { get; set; }
        private IControlPanel Controls { get; set; }
        private readonly GameState GameState;
        private readonly ILog log;

        public InputCommandSource(Queue<AsciiKey> inQ, Describer describer, Window window, GameState state, IControlPanel controls)
        {
            InQ = inQ;
            Describer = describer;
            Window = window;
            GameState = state;
            Controls = controls;
            log = LogManager.GetLogger("CB", "CB.InputCommandSource");
        }
        

        public bool InMultiStepCommand => NextStep != null;
        private Func<AsciiKey, IBeing, Command> NextStep = null;
        private bool QueueIsEmpty => InQ.Count == 0;

        private readonly Command CommandIncomplete = new Command(CmdAction.Incomplete, CmdDirection.None);
        private const int lowercase_a = 97;
        private const int lowercase_z = 123;

        public void GiveCommand(IBeing being)
        {
            bool deliverCommandFromInput()
            {
                var cmd = GetCommand(being);
                if (cmd.Action == CmdAction.Incomplete) return false;

                var actionWasTaken = Controls.CommandBeing(being, cmd);
                if (actionWasTaken) NextStep = null;

                return actionWasTaken;
            }

            var commandGiven = deliverCommandFromInput();
            if (!commandGiven)
            {
                Controls.PushEngineMode(EngineMode.InputBound, deliverCommandFromInput);
            }
        }

        internal Command GetCommand(IBeing being)
        {
            if (QueueIsEmpty) return CommandIncomplete;
            var press = InQ.Dequeue();

            if (InMultiStepCommand)  //  Most of them
            {
                return NextStep(press, being);
            }

            //  Going somewhere?
            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                return new Command(CmdAction.Direction, dir);
            }

            switch (press.Key)
            {
            case Keys.C: return Consume(being);
            case Keys.D: return Drop(being);
            case Keys.H: return Help();
            case Keys.I: return Inventory(being);
            case Keys.U: return Use(being);
            case Keys.W: return Wield(being);
            case Keys.OemComma: return PickUp(being);

            default:
                WriteLine($"Command [{press.Character}] is unknown.");
                return CommandIncomplete;
            }
        }

        public Command Consume(IBeing being)
        {
            var inv_consumables = being.Inventory.Where(i => i.IsConsumable).ToList();
            //var reach_consumables = actor.ReachableItems().Where(i => i.IsConsumable).ToList();
            if (inv_consumables.Count() == 0) /*+ reach_consumables.Count()*/
            {
                WriteLine("Nothing to eat or drink.");
                return CommandIncomplete;
            }
            return FFwdOrPrompt(Consume_main, "Do_Consume (inventory letter or ? to show inventory): ", being);
        }
        public Command Consume_main(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape)
            {
                WriteLine("Do_Consume cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

            if (press.Character == '?')
            {
                ShowInventory(being, i => i.IsConsumable);
                return CommandIncomplete;
            }

            var item = ItemInInventoryLocation(press, being);
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
                WriteLine($"Nothing in inventory slot {press.Character}.");
            }
            return CommandIncomplete;
        }

        public Command Drop(IBeing being)
        {
            if (being.Inventory.Count() == 0)
            {
                WriteLine("Nothing to drop.");
                return CommandIncomplete;
            }
            return FFwdOrPrompt(Drop_main, "Drop (inventory letter or ? to show inventory): ", being);
        }
        public Command Drop_main(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape)
            {
                WriteLine("Drop cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

            //0.0 omae wa mou shindeiru
            if (press.Key == Keys.OemQuestion)
            {
                ShowInventory(being, i => true);
                return CommandIncomplete;
            }

            var item = ItemInInventoryLocation(press, being);
            if (item != null)
            {
                NextStep = null;
                return new Command(CmdAction.Drop, CmdDirection.None, item);
            }
            else
            {
                WriteLine($"Nothing in inventory slot {press.Character}.");
            }

            return CommandIncomplete;
        }
        //HMMM:  Consume_main and Drop_main differ only in CmdAction, inventory filter, text

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

        public Command Inventory(IBeing being)
        {
            ShowInventory(being, i => true);
            return CommandIncomplete;
        }

        #region Use
        private IItem PriorUsedItem = null;
        private IItem ThisUsedItem = null;
        public Command Use(IBeing being)
        {
            ThisUsedItem = ThisUsedItem ?? PriorUsedItem ?? being.WieldedTool;
            if (ThisUsedItem == null)
            {
                ShowInventory(being, i => i.IsUsable);
                return FFwdOrPrompt(Use_Pick_Item, "Use item: ", being);
            }

            return FFwdOrPrompt(Use_Has_Item, 
                $"Direction to use the {Describer.Describe(ThisUsedItem)}, or [a-z?] to choose item: ", being);
        }
        public Command Use_Pick_Item(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape)
            {
                WriteLine("cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

            var selectedIndex = AlphaIndexOfKeyPress(press);
            if (selectedIndex < 0 || being.Inventory.Count() <= selectedIndex)
            {
                WriteLine($"The key [{PressRep(press)}] does not match an inventory item.  Pick another.");
                return CommandIncomplete;
            }

            var item = being.Inventory.ElementAt(selectedIndex);
            if (!item.IsUsable)
            {
                WriteLine($"The {Describer.Describe(item)} is not a usable item.  Pick another.");
                return CommandIncomplete;
            }

            ThisUsedItem = item;

            return FFwdOrPrompt( Use_Has_Item, 
                $"Direction to use the {Describer.Describe(ThisUsedItem)}, or [a-z?] to choose item: ", being);
        }
        private Command Use_Has_Item(AsciiKey press, IBeing being)
        {
            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                NextStep = null;
                PriorUsedItem = ThisUsedItem;
                ThisUsedItem = null;
                return new Command(CmdAction.Use, dir, PriorUsedItem);
            }

            //0.0
            if (press.Key == Keys.OemQuestion)
            {
                ShowInventory(being, i => i.IsUsable);
            }

            if ('a' <= press.Character && press.Character <= 'z')
            {
                return Use_Pick_Item(press, being);
            }

            if (press.Key == Keys.Escape)
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

        public Command Wield(IBeing being)
        {
            var wieldables = being.Inventory.ToList();
            if (wieldables.Count() == 0)
            {
                WriteLine("Nothing to wield.");
                return CommandIncomplete;
            }
            return FFwdOrPrompt(Wield_main, "Wield (inventory letter or ? to show inventory): ", being);
        }

        public Command Wield_main(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape)
            {
                WriteLine("Wield cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

            //0.0
            if (press.Key == Keys.OemQuestion)
            {
                ShowInventory(being, i => true);
                return CommandIncomplete;
            }

            var item = ItemInInventoryLocation(press, being);
            if (item != null)
            {
                NextStep = null;
                return new Command(CmdAction.Wield, CmdDirection.None, item);
            }
            else
            {
                WriteLine($"Nothing in inventory slot {press.Character}.");
            }
            return CommandIncomplete;
        }

        /// <summary> 0.1:  simply grab the topmost.  Later, choose. </summary>
        public Command PickUp(IBeing being)
        {
            var items = GameState.Map.ItemMap.GetItems(being.Position);

            var topItem = items.LastOrDefault();
            if (topItem == null)
            {
                WriteLine("Nothing to pick up here.");
                return CommandIncomplete;
            }

            return new Command(CmdAction.PickUp, CmdDirection.None, topItem);
        }

        #region Utility
        private static string PressRep(AsciiKey press)
        {
            string rep = press.Character.ToString();
            if (string.IsNullOrEmpty(rep)) rep = press.Key.ToString();
            return rep;
        }

        public CmdDirection DirectionOf(AsciiKey press)
        {
            switch (press.Key)
            {
            case Keys.Left: return CmdDirection.West;
            case Keys.Right: return CmdDirection.East;
            case Keys.Up: return CmdDirection.North;
            case Keys.Down: return CmdDirection.South;
            case Keys.NumPad1: return CmdDirection.Southwest;
            case Keys.NumPad2: return CmdDirection.South;
            case Keys.NumPad3: return CmdDirection.Southeast;
            case Keys.NumPad4: return CmdDirection.West;
            case Keys.NumPad6: return CmdDirection.East;
            case Keys.NumPad7: return CmdDirection.Northwest;
            case Keys.NumPad8: return CmdDirection.North;
            case Keys.NumPad9: return CmdDirection.Northeast;
            default: return CmdDirection.None;
            }
        }

        private IItem ItemInInventoryLocation(AsciiKey press, IBeing being)
        {
            int inventorySlot = AlphaIndexOfKeyPress(press);
            if (-1 < inventorySlot && inventorySlot < being.Inventory.Count())
            {
                return being.Inventory.ElementAt(inventorySlot);
            }
            return null;
        }

        private static int AlphaIndexOfKeyPress(AsciiKey press)
        {
            //if (!press.Char.HasValue) return -1;
            var asciiNum = (int)press.Character;
            if (asciiNum < lowercase_a || lowercase_z < asciiNum) return -1;
            return asciiNum - lowercase_a;
        }

        public void Prompt(string text)
        {
            log.InfoFormat("Prompt:  [{0}]", text);

            //0.0 logwindow
            //Window.Prompt(text);
        }

        /// <summary> If more input is queued, the prompt will not be sent </summary>
        private Command FFwdOrPrompt(Func<AsciiKey, IBeing, Command> nextStep, string prompt, IBeing being)
        {
            NextStep = nextStep;
            if (QueueIsEmpty)
            {
                Prompt(prompt);
                return CommandIncomplete;
            }
            return NextStep(InQ.Dequeue(), being);
        }

        public void ShowInventory(IBeing being, Func<IItem, bool> filter = null)
        {
            //0.0 logwindow
            //Window.ShowInventory(actor.Inventory, filter);
        }

        private void WriteLine(string line)
        {
            //0.0 logwindow
            //Window.WriteLine(line);
        }
        #endregion
    }
}
