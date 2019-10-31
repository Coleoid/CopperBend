﻿using System;
using System.Linq;
using log4net;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using SadConsole.Input;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Engine
{
    public class InputCommandSource : ICommandSource
    {
        private readonly Command CommandIncomplete = new Command(CmdAction.Incomplete, CmdDirection.None);
        private readonly ILog log;

        private readonly Describer Describer;
        private readonly GameState GameState;
        private readonly IControlPanel Controls;

        public InputCommandSource(Describer describer, GameState state, IControlPanel controls, ILog logger)
        {
            Describer = describer;
            GameState = state;
            Controls = controls;
            log = logger;
        }

        public bool InMultiStepCommand => NextStep != null;
        private Func<AsciiKey, IBeing, Command> NextStep = null;

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

        public Command GetCommand(IBeing being)
        {
            if (!Controls.IsInputReady()) return CommandIncomplete;
            var press = Controls.GetNextInput();

            if (InMultiStepCommand)  //  Most of them
            {
                return NextStep(press, being);
            }

            //  Going somewhere?
            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                return Direction(being, dir);
            }

            switch (press.Key)
            {
            case Keys.C: return Consume(being);
            case Keys.D: return Drop(being);
            case Keys.H: return Help();
            case Keys.I: return Inventory(being);
            case Keys.S: return SaveGame();
            case Keys.U: return Use(being);
            case Keys.W: return Wield(being);
            case Keys.OemComma: return PickUp(being);

            default:
                if (press.Character != default(char))
                    WriteLine($"Command [{press.Character}] is unknown.");
                return CommandIncomplete;
            }
        }

        private Command Direction(IBeing being, CmdDirection dir)
        {
            var newPosition = Controls.CoordInDirection(being.Position, dir);
            
            var targetBlight = GameState.Map.BlightMap.GetItem(newPosition);
            if (targetBlight != null)
            {
                //0.1.STORY  improve impact of this landmark event
                if (!GameState.Story.HasClearedBlight)
                {
                    blightDirection = dir;
                    Controls.WriteLine("Lookin' at the scum covering the ground sets my teeth on edge.  I'm growling.");
                    return FFwdOrPrompt(Direction_decide_to_Clear_Blight, "Am I goin' after this stuff bare-handed? (y/n): ", being);
                }
            }

            return new Command(CmdAction.Direction, dir);
        }

        //0.1.STORY  first time clearing blight
        CmdDirection blightDirection;
        public Command Direction_decide_to_Clear_Blight(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape || press.Character == 'n')
            {
                WriteLine("Not yet.");
                return CommandIncomplete;
            }

            GameState.Story.HasClearedBlight = true;
            WriteLine("Yes.  Now.");
            return new Command(CmdAction.Direction, blightDirection);
        }


        public Command Consume(IBeing being)
        {
            var inv_consumables = being.Inventory.Where(i => i.IsIngestible).ToList();
            //var reach_consumables = actor.ReachableItems().Where(i => i.IsConsumable).ToList();
            if (inv_consumables.Count() == 0) /*+ reach_consumables.Count()*/
            {
                WriteLine("Nothing to eat or drink on me.");
                return CommandIncomplete;
            }
            return FFwdOrPrompt(Consume_main, "Consume (inventory letter or ? to show inventory): ", being);
        }
        public Command Consume_main(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape)
            {
                WriteLine("Consume cancelled.");
                NextStep = null;
                return CommandIncomplete;
            }

            if (press.Character == '?')
            {
                ShowInventory<IIngestible>(being);
                return CommandIncomplete;
            }

            var item = ItemInInventoryLocation(press, being);
            if (item != null)
            {
                if (item.IsIngestible)
                {
                    NextStep = null;
                    return new Command(CmdAction.Consume, CmdDirection.None, item);
                }
                else
                {
                    WriteLine($"I can't eat or drink {Describer.Describe(item, DescMods.Article)}.");
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

            if (press.Character == '?')
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
            WriteLine("d)rop an item");
            WriteLine("h)elp (or ?) shows this message");
            WriteLine("i)nventory");
            WriteLine("u)se tool, wielded tool by default");
            WriteLine("w)ield a tool");
            WriteLine(",) Pick up object");
            return CommandIncomplete;
        }

        public Command Inventory(IBeing being)
        {
            ShowInventory(being);
            return CommandIncomplete;
        }

        public Command SaveGame()
        {
            //var x 
            return CommandIncomplete;
        }

        #region Use
        private IItem PriorUsedItem = null;
        private IItem ThisUsedItem = null;
        public Command Use(IBeing being)
        {
            //0.2: Next, usability in context?  Or good enough?
            var inv_usables = being.Inventory.Where(i => i.IsUsable).ToList();
            //var reach_usables = actor.ReachableItems().Where(i => i.IsUsable).ToList();
            if (inv_usables.Count() == 0) /*+ reach_usables.Count()*/
            {
                WriteLine("Nothing usable on me.");
                return CommandIncomplete;
            }

            ThisUsedItem = ThisUsedItem ?? PriorUsedItem ?? being.WieldedTool;
            if (ThisUsedItem == null)
            {
                if (!Controls.IsInputReady())
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

            //1.+: 'Use' inventory suggesting uses?
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

            //0.2: Perhaps only 'likely' wields by default?
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

        /// <summary> 0.2:  Currently, grab the topmost.  Later, choose. </summary>
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
            string rep;
            if (press.Character == 0)
            {
                rep = press.Key.ToString();
            }
            else if (20 <= press.Character && press.Character <= 127)
            {
                rep = press.Character.ToString();
            }
            else
            {
                rep = "???";
            }
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
            int asciiNum = press.Character;
            if ('a' <= asciiNum && asciiNum <= 'z')
                return asciiNum - 'a';
            else if ('A' <= asciiNum && asciiNum <= 'Z')
                return asciiNum - 'A';

            return -1;
        }

        /// <summary> If more input is ready, skip prompt and go to the next step </summary>
        private Command FFwdOrPrompt(Func<AsciiKey, IBeing, Command> nextStep, string prompt, IBeing being)
        {
            NextStep = nextStep;
            if (!Controls.IsInputReady())
            {
                Controls.Prompt(prompt);
                return CommandIncomplete;
            }
            return NextStep(Controls.GetNextInput(), being);
        }

        public bool ShowInventory(IBeing being, Func<IItem, bool> filter = null)
        {
            if (filter == null) filter = (i) => true;
            char index = 'a';
            foreach (var item in being.Inventory)
            {
                if (filter(item))
                    WriteLine($"{index}) {item.Name}");

                // if the item is skipped by the filter, the letter still advances.
                index++;
            }
            if (index == 'a')
                WriteLine("Got nothing on me.");

            return index != 'a';
        }

        public void ShowInventory<T>(IBeing being)
        {
            ShowInventory(being, (i) => i.Aspects.HasComponent<T>());
        }

        private void WriteLine(string line)
        {
            Controls.WriteLine(line);
        }
        #endregion
    }
}
