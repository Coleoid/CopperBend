using System;
using System.Diagnostics;
using System.Linq;
using log4net;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Logic
{
    /// <summary> This is the main logic slice of the CommandDispatcher. </summary>
    public partial class CommandDispatcher : IControlPanel
    {
        protected ISpaceMap SpaceMap => GameState.Map.SpaceMap;
        private MultiSpatialMap<IBeing> BeingMap => GameState.Map.BeingMap;
        private IItemMap ItemMap => GameState.Map.ItemMap;
        private IRotMap RotMap => GameState.Map.RotMap;

        // becomes external dependency soon
        public IAttackSystem AttackSystem { get; set; }

        private ISchedule Schedule { get; set; }
        public IGameState GameState { get; set; }
        private readonly IDescriber describer;
        private readonly Action<string> writeLine;
        private readonly ILog log;

        public CommandDispatcher(
            ISchedule schedule,
            IGameState gameState,
            IDescriber describer,
            Action<string> writeLine,
            ILog logger
        )
        {
            log = logger;
            Schedule = schedule;
            GameState = gameState;
            this.describer = describer;
            this.writeLine = writeLine;
            AttackSystem = new AttackSystem(this, logger);
            AttackSystem.RotMap = gameState.Map.RotMap;

            WriteLineIfPlayer = (being, message) => { if (being.IsPlayer) writeLine(message); };
        }

        public void Dispatch(ScheduleEntry nextAction)
        {
            switch (nextAction.Action)
            {
            case ScheduleAction.GetCommand:
                var being = nextAction.Agent as IBeing;
                being.GiveCommand();
                break;

            case ScheduleAction.SeedGrows:
                break;
            case ScheduleAction.PlantGrows:
                break;

            case ScheduleAction.Unset:
                //0.2.DEBUG: create a schedule dump routine, add it to exceptions
                throw new Exception("Unset action got into schedule somehow.");

            default:
                throw new Exception($"Need to write Dispatch() case for ScheduleAction.{nextAction.Action}.");
            }
        }


        public bool CommandBeing(IBeing being, Command command)
        {
            return command.Action switch
            {
                CmdAction.Consume => Do_Consume(being, command),
                CmdAction.Direction => Do_Direction(being, command.Direction),
                CmdAction.Drop => Do_Drop(being, command),
                CmdAction.PickUp => Do_PickUp(being, command),
                CmdAction.Use => Do_Usable(being, command),
                CmdAction.Wait => Do_Wait(being, command),
                CmdAction.Wield => Do_Wield(being, command.Item),
                _ => throw new Exception($"Not ready to do {command.Action}."),
            };
        }


        public bool Do_Consume(IBeing being, Command command)
        {
            if (!(command.Usable is IIngestible ingestible))
                ingestible = command.Item.Aspects.GetComponent<IIngestible>();
            Guard.AgainstNullArgument(ingestible, "Nothing ingestible in consume command");

            IItem item = command.Item;
            Guard.AgainstNullArgument(item, "No item in consume command");
            string description = describer.Describe(item);
            Guard.Against(item.Quantity < 1, $"Only have {item.Quantity} {description}.");
            //0.2: eat from ground, or directly from plant, or someone feeds someone
            Guard.Against(!being.HasInInventory(item), $"{description} to consume not found in inventory");

            Guard.Against(ingestible == null, $"{description} is not ingestible");

            Do_Usable(being, command);

            if (ingestible.IsFruit)
            {
                var plantType = Engine.Compendium.Herbal.PlantByID[ingestible.PlantID];

                // pocket some seeds
                int seedCount = 2; //0.1
                if (seedCount > 0)
                {
                    var seeds = Equipper.BuildPlant("seed", plantType);
                    seeds.Quantity = seedCount;
                    being.AddToInventory(seeds);
                }

                // identify
                plantType.FruitKnown = true;
                plantType.SeedKnown |= seedCount > 0;
                AddExperience(plantType.ID, Exp.EatFruit);
                //0.K: Later, some plants remain mysterious?
            }

            return true;
        }

        #region Direction
        private bool Do_Direction(IBeing being, CmdDirection direction)
        {
            var newPosition = CoordInDirection(being.Position, direction);

            IDefender target = BeingMap.GetItems(newPosition).FirstOrDefault();

            if (target == null)
                target = RotMap.GetItem(newPosition);

            if (target == null)
                return Do_Move(being, newPosition);

            return Do_Attack(being, target, newPosition);
        }


        private bool Do_Attack(IBeing being, IDefender target, Coord position)
        {
            var attackMethod = being.GetAttackMethod(target);
            var defenseMethod = target.GetDefenseMethod(attackMethod);
            AttackSystem.AddAttack(being, attackMethod, target, defenseMethod);
            AttackSystem.ResolveAttackQueue();

            GameState.MarkDirtyCoord(position);
            ScheduleAgent(being, 12);  //0.1 time spent should vary, sched should go elsewhere
            return true;
        }

        private bool Do_Move(IBeing being, Coord newPosition)
        {
            if (newPosition.X < 0 || newPosition.Y < 0
                || SpaceMap.Width <= newPosition.X || SpaceMap.Height <= newPosition.Y)
            {
                //0.1.MAP  add transitions to other maps
                WriteLineIfPlayer(being, "Can't move off the map.");
                return false;
            }
            ISpace space = SpaceMap.GetItem(newPosition);

            if (space.Terrain.Name == "closed door")
            {
                var success = SpaceMap.OpenDoor(space);
                if (success)
                {
                    ScheduleAgent(being, 4);
                    GameState.MarkDirtyCoord(newPosition);
                    GameState.Map.VisibilityChanged = true;
                }
                else
                {
                    writeLine("Door's stuck.");
                }
                return success;
            }

            if (!SpaceMap.CanWalkThrough(newPosition))
            {
                var np = describer.Describe(space.Terrain.Name, DescMods.Article);
                WriteLineIfPlayer(being, $"I can't walk through {np}.");

                ClearPendingInput();
                return false;
            }

            int directionsMoved = 0;
            if (being.Position.X != newPosition.X) directionsMoved++;
            if (being.Position.Y != newPosition.Y) directionsMoved++;
            if (directionsMoved == 0)
                throw new Exception("Moved nowhere?  Up/Down not yet handled.");
            else if (directionsMoved == 1)
                ScheduleAgent(being, 12);
            else
                ScheduleAgent(being, 17);  //0.2  get rid of constant move costs

            being.Position = newPosition;

            if (being.IsPlayer)
            {
                PlayerMoved = true;
                var itemsHere = ItemMap.GetItems(newPosition);
                if (itemsHere.Count() > 7)
                {
                    writeLine("A pile of things here.");
                }
                else if (itemsHere.Count() > 1)
                {
                    writeLine("Some things here.");
                }
                else if (itemsHere.Count() == 1)
                {
                    var item = itemsHere.ElementAt(0);
                    var beVerb = item.Quantity == 1 ? "is" : "are";
                    var np = describer.Describe(item, DescMods.Article);
                    writeLine($"There {beVerb} {np} here.");
                }
                else
                {
                } //  Nothing here, report nothing
            }

            return true;
        }
        #endregion

        private bool Do_Drop(IBeing being, Command command)
        {
            Guard.AgainstNullArgument(command.Item, "No item in drop command");
            var item = being.RemoveFromInventory(command.Item);
            Guard.AgainstNullArgument(item, "Item to drop not found in inventory");

            item.MoveTo(being.Position);
            ItemMap.Add(item, item.Location);
            ScheduleAgent(being, 1);
            return true;
        }

        private bool Do_PickUp(IBeing being, Command command)
        {
            var item = command.Item;
            Guard.AgainstNullArgument(item, "No item chosen to pick up.");
            var pickedUp = ItemMap.Remove(item);
            if (pickedUp)
            {
                being.AddToInventory(item);
                ScheduleAgent(being, 4);
                writeLine($"Picked up {item.Name}");
            }
            else
            {
                writeLine($"Item {item.Name} was no longer on the map, to pick up");
            }
            return pickedUp;
        }

        private int time_spent_on_usable = 0;
        private bool Do_Usable(IBeing being, Command command)
        {
            Guard.AgainstNullArgument(command.Usable, "Need Usable component of command");

            time_spent_on_usable = 0;
            bool action_taken = false;

            foreach (var effect in command.Usable.Effects)
            {
                switch (effect.Effect)
                {
                case "till":
                    action_taken |= Till(being, command);
                    break;

                case "plant":
                    action_taken |= Plant(being, command);
                    break;

                case "food":
                    FeedBeing(being, effect.Amount);
                    action_taken = true;
                    break;

                case "heal":
                    HealBeing(being, effect.Amount);
                    action_taken = true;
                    break;

                default:
                    log.Error("usable isn't.");
                    var vp = command.Usable.VerbPhrase;
                    var np = describer.Describe(command.Item, DescMods.Article);
                    throw new Exception($"Don't have code to cause [{effect.Effect}] Effect when I {vp} {np}.");
                }
            }

            //  Usable.Costs are contingent on success of .Effects
            if (action_taken)
            {
                foreach (var cost in command.Usable.Costs)
                {
                    PayCost(being, command.Item, cost);
                }

                if (time_spent_on_usable > 0)
                    ScheduleAgent(being, time_spent_on_usable);
            }

            return action_taken;
        }

        public void PayCost(IBeing being, IItem item, UseCost cost)
        {
            switch (cost.Substance)
            {
            case "health":
                being.Hurt(cost.Amount);
                break;

            case "energy":
                being.Fatigue(cost.Amount);
                break;

            case "time":
                time_spent_on_usable += cost.Amount;
                break;

            case "this":
                item.Quantity -= cost.Amount;
                if (item.Quantity < 1)
                    being.RemoveFromInventory(item);
                break;

            default:
                throw new Exception($"Don't know how to pay Usable cost [{cost.Substance}] yet.");
            }
        }

        private bool Till(IBeing being, Command command)
        {
            var targetCoord = CoordInDirection(being.Position, command.Direction);
            var space = SpaceMap.GetItem(targetCoord);
            if (space.IsTilled)
            {
                WriteLineIfPlayer(being, "Ground here's already tilled.");
                return false;
            }

            if (!space.CanTill)
            {
                WriteLineIfPlayer(being, $"Cannot till the {space.Terrain.Name}.");
                return false;
            }

            if (being.WieldedTool != command.Item)
            {
                being.Wield(command.Item);
                PayCost(being, command.Item, new UseCost("time", 6));
            }

            Till(space);
            GameState.MarkDirtyCoord(targetCoord);
            return true;
        }

        private bool Plant(IBeing being, Command command)
        {
            var coord = CoordInDirection(being.Position, command.Direction);
            var space = SpaceMap.GetItem(coord);

            if (!space.IsTilled)
            {
                string qualifier = space.CanTill ? "untilled " : string.Empty;
                writeLine($"Cannot sow {qualifier}{space.Terrain.Name}.");
                return false;
            }

            if (space.IsSown)
            {
                writeLine($"The ground to my {command.Direction} is already sown with a seed.");
                return false;
            }

            IItem toSow = being.RemoveFromInventory(command.Item, 1);

            SpaceMap.MarkSpaceSown(space);
            GameState.MarkDirtyCoord(coord);

            var growingPlant = toSow.Aspects.GetComponent<Plant>();
            AddExperience(growingPlant.PlantDetails.ID, Exp.PlantSeed);
            ScheduleAgent(growingPlant, 100);

            return true;
        }


#pragma warning disable CA1801 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        private bool Do_Wait(IBeing being, Command command)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1801 // Remove unused parameter
        {
            Schedule.AddAgent(being, 6);
            return true;
        }

        private bool Do_Wield(IBeing being, IItem item)
        {
            being.Wield(item);
            ScheduleAgent(being, 6);
            return true;
        }
    }
}
