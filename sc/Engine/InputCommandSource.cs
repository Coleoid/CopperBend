using System;
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

        public bool IsAssemblingCommand => NextStep != null;
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
                Controls.PushEngineMode(EngineMode.PlayerTurn, deliverCommandFromInput);
            }
        }

        /// <summary>
        /// The InputCommandSource builds a Command based on 
        /// keyboard input.  It may take thousands of Update()
        /// callbacks before a Command other than CommandIncomplete
        /// is returned.  The en
        /// </summary>
        /// <param name="being"></param>
        /// <returns></returns>
        public Command GetCommand(IBeing being)
        {
            // Most of the time, when waiting on a human, we're going
            // to hit this line and return with no input and no command.
            if (!Controls.IsInputReady()) return CommandIncomplete;

            var press = Controls.GetNextInput();

            // Most commands take multiple keystrokes to resolve.
            // When that's true, we've stored the next step to take for when
            // we have the next keystroke(s) to get us closer to a Command.
            if (IsAssemblingCommand) return NextStep(press, being);

            // Otherwise, if we've got a direction key, do that
            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                return Direction(being, dir);
            }

            // Or dispatch the beginning of another Command
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

        public Command SameStep(string text)
        {
            WriteLine(text);
            return CommandIncomplete;
        }

        private Command CancelMultiStep(string text)
        {
            WriteLine(text);
            NextStep = null;
            return CommandIncomplete;
        }

        /// <summary> If more input is ready, skip prompt and go to the next step </summary>
        private Command NextStepIs(Func<AsciiKey, IBeing, Command> nextStep, string prompt, IBeing being)
        {
            NextStep = nextStep;
            if (Controls.IsInputReady()) return NextStep(Controls.GetNextInput(), being);

            Controls.Prompt(prompt);
            return CommandIncomplete;
        }

        private Command FinishedCommand(CmdAction action, CmdDirection direction, IItem item = null, IUsable usable = null)
        {
            NextStep = null;
            return new Command(action, direction, item, usable);
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
                    Controls.WriteLine("The scum covering the ground sets my teeth on edge.  I'm growling.");
                    return NextStepIs(Direction_decide_to_Clear_Blight, "Am I going after this stuff bare-handed? ", being);
                }
            }

            return FinishedCommand(CmdAction.Direction, dir);
        }

        CmdDirection blightDirection;
        public Command Direction_decide_to_Clear_Blight(AsciiKey press, IBeing being)
        {
            //0.1.STORY: change decision mechanic to key == dir towards area blight
            if (press.Key == Keys.Escape || press.Character == 'n')
            {
                WriteLine("Not yet.");
                return CommandIncomplete;
            }

            GameState.Story.HasClearedBlight = true;
            WriteLine("Yes.  Now.");
            return FinishedCommand(CmdAction.Direction, blightDirection);
        }


        public Command Consume(IBeing being)
        {
            //1.+: food on ground, on adjacent plant, drink from pool...
            var inv_consumables = being.Inventory.Where(i => i.IsIngestible).ToList();

            return (inv_consumables.Count() == 0)
                ? CancelMultiStep("Nothing to eat or drink on me.")
                : NextStepIs(Consume_main, "Consume (inventory letter or ? to show inventory): ", being);
        }
        public Command Consume_main(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape) return CancelMultiStep("cancelled.");
            if (press.Character == '?') return ShowInventory<IIngestible>(being);

            var item = ItemInInventoryLocation(press, being);
            if (item == null) return SameStep($"Nothing in inventory slot {press.Character}.");
            if (!item.IsIngestible) return SameStep($"I can't eat or drink {Describer.Describe(item, DescMods.Article)}.");

            return FinishedCommand(CmdAction.Consume, CmdDirection.None, item);
        }

        public Command Drop(IBeing being)
        {
            return (being.Inventory.Count() == 0)
                ? CancelMultiStep("Nothing to drop.")
                : NextStepIs(Drop_main, "Drop (inventory letter or ? to show inventory): ", being);
        }
        public Command Drop_main(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape) return CancelMultiStep("Drop cancelled.");
            if (press.Character == '?') return ShowInventory(being, i => true);

            var item = ItemInInventoryLocation(press, being);
            if (item == null) return SameStep($"Nothing in inventory slot {press.Character}.");

            return FinishedCommand(CmdAction.Drop, CmdDirection.None, item);
        }

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
            return ShowInventory(being);
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
                return CancelMultiStep("Nothing usable on me.");
            }

            ThisUsedItem = ThisUsedItem ?? PriorUsedItem ?? being.WieldedTool;
            if (ThisUsedItem == null)
            {
                if (!Controls.IsInputReady())
                    ShowInventory(being, i => i.IsUsable);
                return NextStepIs(Use_Pick_Item, "Use item: ", being);
            }

            return NextStepIs(Use_Has_Item, 
                $"Direction to use the {Describer.Describe(ThisUsedItem)}, or [a-z?] to choose item: ", being);
        }
        public Command Use_Pick_Item(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape) return CancelMultiStep("cancelled.");

            var selectedIndex = AlphaIndexOfKeyPress(press);
            if (selectedIndex < 0 || being.Inventory.Count() <= selectedIndex)
            {
                return SameStep($"The key [{PressRep(press)}] does not match an inventory item.  Pick another.");
            }

            var item = being.Inventory.ElementAt(selectedIndex);
            if (!item.IsUsable)
            {
                return SameStep($"The {Describer.Describe(item)} is not a usable item.  Pick another.");
            }

            ThisUsedItem = item;

            return NextStepIs( Use_Has_Item, 
                $"Direction to use the {Describer.Describe(ThisUsedItem)}, or [a-z?] to choose item: ", being);
        }

        private Command Use_Has_Item(AsciiKey press, IBeing being)
        {
            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                PriorUsedItem = ThisUsedItem;
                ThisUsedItem = null;
                return FinishedCommand(CmdAction.Use, dir, PriorUsedItem);
            }

            //1.+: 'Use' inventory suggesting uses?
            if (press.Key == Keys.OemQuestion) return ShowInventory(being, i => i.IsUsable);
            if (press.Key == Keys.Escape) return CancelMultiStep("cancelled.");

            if ('a' <= press.Character && press.Character <= 'z')
            {
                return Use_Pick_Item(press, being);
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
            if (wieldables.Count() == 0) return CancelMultiStep("Nothing to wield.");

            return NextStepIs(Wield_main, "Wield (inventory letter or ? to show inventory): ", being);
        }

        public Command Wield_main(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape) return CancelMultiStep("Wield cancelled.");

            //0.2: Perhaps only 'likely' wields by default?
            if (press.Key == Keys.OemQuestion) return ShowInventory(being, i => true);

            var item = ItemInInventoryLocation(press, being);
            if (item == null) return SameStep($"Nothing in inventory slot {press.Character}.");

            return FinishedCommand(CmdAction.Wield, CmdDirection.None, item);
        }

        /// <summary> 0.2:  Currently, grab the topmost.  Later, choose. </summary>
        public Command PickUp(IBeing being)
        {
            var items = GameState.Map.ItemMap.GetItems(being.Position);
            var topItem = items.LastOrDefault();
            if (topItem == null) return CancelMultiStep("Nothing to pick up here.");

            return FinishedCommand(CmdAction.PickUp, CmdDirection.None, topItem);
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

        public Command ShowInventory(IBeing being, Func<IItem, bool> filter = null)
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

            return CommandIncomplete;
        }

        public Command ShowInventory<T>(IBeing being)
        {
            return ShowInventory(being, (i) => i.Aspects.HasComponent<T>());
        }

        private void WriteLine(string line)
        {
            Controls.WriteLine(line);
        }
        #endregion
    }
}
