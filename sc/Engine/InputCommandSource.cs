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
        private readonly Command commandIncomplete = new Command(CmdAction.Incomplete, CmdDirection.None);
        private readonly ILog log;

        private readonly Describer describer;
        private readonly GameState gameState;
        private readonly IControlPanel controls;

        public InputCommandSource(Describer describer, GameState state, IControlPanel controls, ILog logger)
        {
            this.describer = describer;
            this.gameState = state;
            this.controls = controls;
            log = logger;
        }

        public bool IsAssemblingCommand => nextStep != null;
        private Func<AsciiKey, IBeing, Command> nextStep = null;

        public void GiveCommand(IBeing being)
        {
            void DeliverCommandFromInput()
            {
                var cmd = GetCommand(being);
                if (cmd.Action == CmdAction.Incomplete) return;

                var actionWasTaken = controls.CommandBeing(being, cmd);
                if (actionWasTaken)
                {
                    nextStep = null;
                    controls.PopEngineMode();
                }
            }

            controls.PushEngineMode(EngineMode.PlayerTurn, DeliverCommandFromInput);
            DeliverCommandFromInput();
        }

        /// <summary>
        /// The InputCommandSource builds a Command based on
        /// keyboard input.  It may take thousands of Update()
        /// callbacks before a Command other than CommandIncomplete
        /// is returned.
        /// </summary>
        public Command GetCommand(IBeing being)
        {
            // Most of the time, when waiting on a human, we're going
            // to hit this line and return with no input and no command.
            if (!controls.IsInputReady()) return commandIncomplete;

            var press = controls.GetNextInput();

            // Most commands take multiple keystrokes to resolve.
            // When that's true, we've stored the next step to take for when
            // we have the next keystroke(s) to get us closer to a Command.
            if (IsAssemblingCommand) return nextStep(press, being);

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
                return commandIncomplete;
            }
        }

        public Command SameStep(string text)
        {
            WriteLine(text);
            return commandIncomplete;
        }

        private Command CancelMultiStep(string text)
        {
            WriteLine(text);
            nextStep = null;
            return commandIncomplete;
        }

        /// <summary> If more input is ready, skip prompt and go to the next step </summary>
        private Command NextStepIs(Func<AsciiKey, IBeing, Command> nextStep, string prompt, IBeing being)
        {
            this.nextStep = nextStep;
            if (controls.IsInputReady()) return this.nextStep(controls.GetNextInput(), being);

            controls.Prompt(prompt);
            return commandIncomplete;
        }

        private Command FinishedCommand(CmdAction action, CmdDirection direction, IItem item = null, IUsable usable = null)
        {
            nextStep = null;
            return new Command(action, direction, item, usable);
        }


        private Command Direction(IBeing being, CmdDirection dir)
        {
            var newPosition = controls.CoordInDirection(being.Position, dir);

            var targetBlight = gameState.Map.BlightMap.GetItem(newPosition);
            if (targetBlight != null)
            {
                //0.1.STORY  improve impact of this landmark event
                if (!gameState.Story.HasClearedBlight)
                {
                    blightDirection = dir;
                    controls.WriteLine("The filth covering the ground sets my teeth on edge.  I'm growling.");
                    return NextStepIs(Direction_decide_to_Clear_Blight, "Am I going after this stuff bare-handed? ", being);
                }
            }

            return FinishedCommand(CmdAction.Direction, dir);
        }

        private CmdDirection blightDirection;
        public Command Direction_decide_to_Clear_Blight(AsciiKey press, IBeing being)
        {
            //  Decision mechanic: hit 'y', or travel in a direction
            // towards blight (in which case, the decision direction will
            // be the attack direction, even if different from the original
            // direction).  Any other response cancels move/attack.
            bool affirm = press.Key == Keys.Y;

            if (!affirm)
            {
                var dir = DirectionOf(press);
                if (dir != CmdDirection.None)
                {
                    var dirCoord = controls.CoordInDirection(being.Position, dir);
                    var dirBlight = gameState.Map.BlightMap.GetItem(dirCoord);

                    affirm = (dirBlight != null);
                    if (affirm) blightDirection = dir;
                }
            }

            if (affirm)
            {
                gameState.Story.HasClearedBlight = true;
                WriteLine("Yes.  Now.");
                log.Info("Decided to clear Rot.");
                return FinishedCommand(CmdAction.Direction, blightDirection);
            }
            else
            {
                return CancelMultiStep("Not yet.");
            }
        }


        public Command Consume(IBeing being)
        {
            //1.+: food on ground, on adjacent plant, drink from pool...
            var inv_consumables = being.Inventory.Where(i => i.IsIngestible).ToList();

            return (inv_consumables.Count == 0)
                ? CancelMultiStep("Nothing to eat or drink on me.")
                : NextStepIs(Consume_main, "Consume (inventory letter or ? to show inventory): ", being);
        }
        public Command Consume_main(AsciiKey press, IBeing being)
        {
            if (press.Key == Keys.Escape) return CancelMultiStep("cancelled.");
            if (press.Character == '?') return ShowInventory<IIngestible>(being);

            var item = ItemInInventoryLocation(press, being);
            if (item == null) return SameStep($"Nothing in inventory slot {press.Character}.");
            if (!item.IsIngestible) return SameStep($"I can't eat or drink {describer.Describe(item, DescMods.Article)}.");

            return FinishedCommand(CmdAction.Consume, CmdDirection.None, item);
        }

        public Command Drop(IBeing being)
        {
            return (being.Inventory.Count == 0)
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
            return commandIncomplete;
        }

        public Command Inventory(IBeing being)
        {
            return ShowInventory(being);
        }

        public Command SaveGame()
        {
            return commandIncomplete;
        }

        #region Use
        private IItem priorUsedItem = null;
        private IItem thisUsedItem = null;
        public Command Use(IBeing being)
        {
            //0.2: Next, usability in context?  Or good enough?
            var inv_usables = being.Inventory.Where(i => i.IsUsable).ToList();
            //var reach_usables = actor.ReachableItems().Where(i => i.IsUsable).ToList();
            if (inv_usables.Count == 0)
            {
                return CancelMultiStep("Nothing usable on me.");
            }

            thisUsedItem ??= priorUsedItem ?? being.WieldedTool;
            if (thisUsedItem == null)
            {
                if (!controls.IsInputReady())
                    ShowInventory(being, i => i.IsUsable);
                return NextStepIs(Use_Pick_Item, "Use item: ", being);
            }

            return NextStepIs(
                Use_Has_Item,
                $"Direction to use the {describer.Describe(thisUsedItem)}, or [a-z?] to choose item: ", being);
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
                return SameStep($"The {describer.Describe(item)} is not a usable item.  Pick another.");
            }

            thisUsedItem = item;

            return NextStepIs(
                Use_Has_Item,
                $"Direction to use the {describer.Describe(thisUsedItem)}, or [a-z?] to choose item: ", being);
        }

        private Command Use_Has_Item(AsciiKey press, IBeing being)
        {
            var dir = DirectionOf(press);
            if (dir != CmdDirection.None)
            {
                priorUsedItem = thisUsedItem;
                thisUsedItem = null;
                return FinishedCommand(CmdAction.Use, dir, priorUsedItem);
            }

            //1.+: 'Use' inventory suggesting uses?
            if (press.Key == Keys.OemQuestion) return ShowInventory(being, i => i.IsUsable);
            if (press.Key == Keys.Escape) return CancelMultiStep("cancelled.");

            if ('a' <= press.Character && press.Character <= 'z')
            {
                return Use_Pick_Item(press, being);
            }

            WriteLine($"The key [{PressRep(press)}] does not match an inventory item or a direction.  Pick another.");
            return commandIncomplete;

            //  Some of this needs to end up in Action system
            //    var targetPoint = PointInDirection(Player.Point, direction);
            //    WriteLine(direction.ToString());
            //    _usingItem.ApplyTo(Map[targetPoint], this, direction);  // the magic
        }
#endregion

        public Command Wield(IBeing being)
        {
            var wieldables = being.Inventory.ToList();
            if (wieldables.Count == 0) return CancelMultiStep("Nothing to wield.");

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
            var items = gameState.Map.ItemMap.GetItems(being.Position);
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
            return press.Key switch
            {
                Keys.Left => CmdDirection.West,
                Keys.Right => CmdDirection.East,
                Keys.Up => CmdDirection.North,
                Keys.Down => CmdDirection.South,
                Keys.NumPad1 => CmdDirection.Southwest,
                Keys.NumPad2 => CmdDirection.South,
                Keys.NumPad3 => CmdDirection.Southeast,
                Keys.NumPad4 => CmdDirection.West,
                Keys.NumPad6 => CmdDirection.East,
                Keys.NumPad7 => CmdDirection.Northwest,
                Keys.NumPad8 => CmdDirection.North,
                Keys.NumPad9 => CmdDirection.Northeast,
                _ => CmdDirection.None,
            };
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

            return commandIncomplete;
        }

        public Command ShowInventory<T>(IBeing being)
        {
            return ShowInventory(being, (i) => i.Aspects.HasComponent<T>());
        }

        private void WriteLine(string line)
        {
            controls.WriteLine(line);
        }
        #endregion
    }
}
