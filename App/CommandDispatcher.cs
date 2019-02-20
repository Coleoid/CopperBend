using System;
using System.Linq;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    public partial class CommandDispatcher
    {
        private Schedule Schedule { get; set; }

        private IGameState GameState { get; set; }
        private IActor Player { get => GameState.Player; }
        private IAreaMap Map { get => GameState.Map; }

        private Describer Describer;
        private EventBus EventBus;
        private IMessageOutput Output;

        private Action<RLKeyPress> NextStep = null;
        private bool InMultiStepCommand => NextStep != null;

        public CommandDispatcher(
            Schedule schedule, 
            IGameState gameState, 
            Describer describer,
            EventBus bus,
            IMessageOutput messageOutput
            )
        {
            Schedule = schedule;
            GameState = gameState;
            Describer = describer;
            EventBus = bus;
            Output = messageOutput;
        }

        public bool CommandActor(Command command, IActor actor)
        {
            switch (command.Action)
            {
            case CmdAction.Move:
                return Command_Direction(actor, command.Direction);
            case CmdAction.PickUp:
                break;
            case CmdAction.Consume:
                break;
            case CmdAction.Drop:
                command.Item.MoveTo(actor.Point);
                Map.Items.Add(command.Item);
                ScheduleActor(actor, 1);
                break;
            case CmdAction.Use:
                break;
            case CmdAction.Wait:
                break;

            case CmdAction.Unknown:
            case CmdAction.Unset:
            case CmdAction.None:
            default:
                throw new Exception($"Bad action {command.Action}.");
            }

            return false;
        }

        #region Direction

        private bool Command_Direction(IActor actor, CmdDirection direction)
        {
            var point = PointInDirection(actor.Point, direction);

            IActor targetActor = Map.GetActorAtPoint(point);
            if (targetActor == null)
            {
                return Command_DirectionMove(actor, point);
            }
             
            return Command_DirectionAttack(actor, targetActor);
        }

        private bool Command_DirectionMove(IActor actor, Point point)
        {
            ITile tile = Map[point];
            if (tile.TileType.Name == "closed door")
            {
                Map.OpenDoor(tile);
                ScheduleActor(actor, 4);
            }
            else if (!Map.IsWalkable(point))
            {
                var np = Describer.Describe(tile.TileType.Name, DescMods.IndefiniteArticle);
                Output.WriteLine($"I can't walk through {np}.");
                EventBus.ClearPendingInput(this, new EventArgs());
            }
            else
            {
                CheckActorAtCoordEvent(actor, tile);

                if (!Map.MoveActor(actor, point))
                    throw new Exception($"Somehow failed to move onto {point}, a walkable tile.");

                Map.UpdatePlayerFieldOfView(actor);
                Map.IsDisplayDirty = true;
                if (actor.Point.X != point.X && actor.Point.Y != point.Y)
                    ScheduleActor(actor, 17);
                else
                    ScheduleActor(actor, 12);

                var itemsHere = Map.Items.Where(i => i.Point == point);
                if (itemsHere.Count() > 7)
                {
                    Output.WriteLine("There are many items here.");
                }
                else if (itemsHere.Count() > 1)
                {
                    Output.WriteLine("There are several items here.");
                }
                else if (itemsHere.Count() == 1)
                {
                    var item = itemsHere.ElementAt(0);
                    var beVerb = item.Quantity == 1 ? "is" : "are";
                    var np = Describer.Describe(item, DescMods.Quantity);
                    Output.WriteLine($"There {beVerb} {np} here.");
                }
                else {}  //  Nothing here, report nothing
            }

            return true;
        }

        private bool Command_DirectionAttack(IActor actor, IActor target)
        {
            //0.1
            //var conflictSystem = new ConflictSystem(Window, Map, Schedule);
            //conflictSystem.Attack("Wah!", 2, targetActor);

            ScheduleActor(actor, 12);
            return true;
        }

        public void CheckActorAtCoordEvent(IActor actor, ITile tile)
        {
            if (Map.LocationMessages.ContainsKey(tile.Point))
            {
                var message = Map.LocationMessages[tile.Point];
                foreach (var line in message)
                    Output.WriteLine(line);

                Map.LocationMessages.Remove(tile.Point);
            }

            ////0.2
            //if (Map.LocationEventEntries.ContainsKey(tile.Point))
            //{
            //    var entries = Map.LocationEventEntries[tile.Point];
            //    foreach (var entry in entries)
            //    {
            //        //CommandQueue.Enqueue(entry.Command);
            //    }
            //}

            //0.3 may unify those collections and loops, may restructure flow
        }
        #endregion

        #region Consume
        private void Consume_Prompt(RLKeyPress key)
        {
            Output.Prompt("Consume (inventory letter or ? to show inventory): ");
            NextStep = Consume_Main;
        }

        private void Consume_Main(RLKeyPress key)
        {
            //  Bail out
            if (key.Key == RLKey.Escape)
            {
                Output.WriteLine("nothing.");
                NextStep = null;
                return;
            }

            //  Show inventory, re-prompt, wait for selection
            if (key.Key == RLKey.Slash && key.Shift)
            {
                Command_Inventory();
                Consume_Prompt(null);
                return;
            }

            int inventorySlot = AlphaIndexOfKeyPress(key);
            if (inventorySlot == -1) return;

            var item = Player.Inventory.ElementAt(inventorySlot);
            if (!item.IsConsumable)
            {
                Output.WriteLine($"I can't {item.ConsumeVerb} {Describer.Describe(item, DescMods.IndefiniteArticle)}.");
                Consume_Prompt(null);
                return;
            }

            item.Consume((IControlPanel)this);

            NextStep = null;
        }
        #endregion


        private void Command_Help()
        {
            Output.WriteLine("Help:");
            Output.WriteLine("Arrow or numpad keys to move and attack");
            Output.WriteLine("a)pply wielded tool");
            Output.WriteLine("d)rop an item");
            Output.WriteLine("h)elp (or ?) shows this message");
            Output.WriteLine("i)nventory");
            Output.WriteLine("w)ield a tool");
            Output.WriteLine(",) Pick up object");
        }

        private void Command_Inventory()
        {
            Output.WriteLine("Inventory:");
            if (Player.Inventory.Count() == 0)
            {
                Output.WriteLine("empty.");
            }
            else if (Player.Inventory.Count() < 8)
            {
                int asciiSlot = lowercase_a;
                foreach (var item in Player.Inventory)
                {
                    var description = Describer.Describe(item, DescMods.Quantity | DescMods.IndefiniteArticle);
                    Console.WriteLine($"{(char)asciiSlot})  {description}");
                    asciiSlot++;
                }
            }
            else
            {
                //  Bring up an inventory console
                throw new Exception("I really need an inventory console now.");
            }
        }

        #region Use Item
        private void Use_Prompt(RLKeyPress key)
        {
            if (_usingItem == null) _usingItem = Player.WieldedTool;
            if (_usingItem == null)
            {
                Use_Prompt_Choose(null);
                return;
            }

            Output.Prompt($"Pick direction to use {_usingItem.Name}, or pick another item with a-z or ?: ");
            NextStep = Use_in_Direction;
        }

        private void Use_Prompt_Choose(RLKeyPress key)
        {
            Command_Inventory();
            Output.Prompt("Pick an item to use: ");
            NextStep = Use_Choose_item;
        }

        private IItem _usingItem;
        private void Use_in_Direction(RLKeyPress key)
        {
            if (key.Key == RLKey.Escape)
            {
                Output.WriteLine("cancelled.");
                NextStep = null;
                return;
            }

            if (key.Key == RLKey.Slash && key.Shift)
            {
                Use_Prompt_Choose(null);
                return;
            }

            if (key.Char >= 'a' && key.Char <= 'z')
            {
                Use_Choose_item(key);
                return;
            }

            var direction = DirectionOfKey(key);
            if (direction != Direction.None)
            {
                var targetPoint = PointInDirection(Player.Point, direction);
                Output.WriteLine(direction.ToString());

                _usingItem.ApplyTo(Map[targetPoint], this, Output, direction);  // the magic
                //int tickOff = TimeCostToApply(_usingItem, actor.Skills);
                //ScheduleActor(actor, tickOff);
                NextStep = null;
            }
        }

        private void Use_Choose_item(RLKeyPress key)
        {
            if (key.Key == RLKey.Escape)
            {
                Output.WriteLine("cancelled.");
                NextStep = null;
                return;
            }

            var selectedIndex = AlphaIndexOfKeyPress(key);
            if (selectedIndex < 0 || Player.Inventory.Count() <= selectedIndex)
            {
                Output.WriteLine($"The key [{key.Char}] does not match an inventory item.  Pick another.");
                return;
            }

            var item = Player.Inventory.ElementAt(selectedIndex);
            if (!item.IsUsable)
            {
                Output.WriteLine($"The [{item.Name}] is not a usable item.  Pick another.");
                return;
            }

            _usingItem = item;
            Output.WriteLine($"Using {item.Name} in what direction: ");
            NextStep = Use_in_Direction;
        }
        #endregion

        #region Wield
        private void Wield_Prompt(RLKeyPress key)
        {
            Output.Prompt("Wield ('?' to show inventory, '.' to empty hands): ");
            NextStep = Wield_Choose;
        }

        private void Wield_Choose(RLKeyPress key)
        {
            //  Escape:  Bail out unchanged
            if (key.Key == RLKey.Escape)
            {
                var name = Player.WieldedTool == null
                    ? "bare hands"
                    : Player.WieldedTool.Name;
                Output.WriteLine($"unchanged, {name}.");
                NextStep = null;
                return;
            }

            //  Period:  Wield empty hands, done
            if (key.Key == RLKey.Period)
            {
                Output.WriteLine("bare hands");
                Player.Wield(null);
                NextStep = null;
                return;
            }

            //  Question mark:  Show inventory, reprompt
            if (key.Key == RLKey.Slash && key.Shift)
            {
                Command_Inventory();
                Wield_Prompt(null);
                return;
            }

            //  If not a good inventory choice, warn and reprompt
            int inventorySlot = AlphaIndexOfKeyPress(key);
            if (inventorySlot == -1) return;
            if (inventorySlot >= Player.Inventory.Count())
            {
                Output.WriteLine($"nothing in slot '{key.Char.Value}'.");
                Wield_Prompt(null);
                return;
            }

            // we can wield even silly stuff, until it's a problem.
            var item = Player.Inventory.ElementAt(inventorySlot);

            Player.Wield(item);
            Output.WriteLine(item.Name);
            //ScheduleActor(4);
            NextStep = null;
        }
        #endregion

        private void Command_PickUp()
        {
            var topItem = Map.Items
                .Where(i => i.Point.Equals(Player.Point))
                .LastOrDefault();

            if (topItem == null)
            {
                Output.WriteLine("Nothing to pick up here.");
                return;
            }

            Map.Items.Remove(topItem);
            Player.AddToInventory(topItem);
            Output.WriteLine($"Picked up {topItem.Name}");
            //ScheduleActor(2);
        }
    }
}
