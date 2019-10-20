using System;
using System.Linq;
using log4net;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Engine
{
    public partial class CommandDispatcher
    {
        private ISchedule Schedule { get; set; }
        private IGameState GameState { get; set; }

        protected ISpaceMap SpaceMap => GameState.Map.SpaceMap;
        private MultiSpatialMap<IBeing> BeingMap => GameState.Map.BeingMap;
        private IItemMap ItemMap => GameState.Map.ItemMap;
        private IBlightMap BlightMap => GameState.Map.BlightMap;

        private IDescriber Describer;
        private IMessageLogWindow MessageLog;
        private ILog log;

        // becomes external dependency soon
        public AttackSystem AttackSystem { get; set; }

        public CommandDispatcher(
            ISchedule schedule,
            IGameState gameState,
            IDescriber describer,
            IMessageLogWindow messageLog
        )
        {
            Schedule = schedule;
            GameState = gameState;
            Describer = describer;
            MessageLog = messageLog;
            AttackSystem = new AttackSystem(this);
            WriteLineIfPlayer = (being, message) => { if (being.IsPlayer) MessageLog.WriteLine(message); };
            log = LogManager.GetLogger("CB", "CB.Dispatcher");
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
            switch (command.Action)
            {
            case CmdAction.Consume:   return Do_Consume(being, command);
            case CmdAction.Direction: return Do_Direction(being, command.Direction);
            case CmdAction.Drop:      return Do_Drop(being, command);
            case CmdAction.PickUp:    return Do_PickUp(being, command);
            case CmdAction.Use:       return Do_Usable(being, command);
            case CmdAction.Wait:      return Do_Wait(being, command);
            case CmdAction.Wield:     return Do_Wield(being, command.Item);

            case CmdAction.Unknown:
            case CmdAction.Unset:
            case CmdAction.Incomplete:
            default:
                throw new Exception($"Not ready to do {command.Action}.");
            }
        }


        public bool Do_Consume(IBeing being, Command command)
        {
            IIngestible ingestible = command.Usable as IIngestible;
            if (ingestible == null)
                ingestible = command.Item.Aspects.GetComponent<IIngestible>();
            Guard.AgainstNullArgument(ingestible, "Nothing ingestible in consume command");

            IItem item = command.Item;
            Guard.AgainstNullArgument(item, "No item in consume command");
            string description = Describer.Describe(item);
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
                target = BlightMap.GetItem(newPosition);

            if (target == null)
                return Do_DirectionMove(being, newPosition);

            return Do_Attack(being, target, newPosition);
        }


        private bool Do_Attack(IBeing being, IDefender target, Coord position)
        {
            var attackMethod = being.GetAttackMethod(target);
            AttackSystem.AddAttack(being, attackMethod, target, null); //0.1: need defense
            AttackSystem.ResolveAttackQueue();

            GameState.MarkDirtyCoord(position);
            ScheduleAgent(being, 12);  //0.2 attack time spent should vary
            return true;
        }

        //0.0.DMG:  Our hero is oddly resistant to the effects of the blight

        private bool Do_DirectionMove(IBeing being, Coord newPosition)
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
                }
                else
                {
                    MessageLog.WriteLine("Door's stuck.");
                }
                return success;
            }

            if (!SpaceMap.CanWalkThrough(newPosition))
            {
                var np = Describer.Describe(space.Terrain.Name, DescMods.Article);
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
                    MessageLog.WriteLine("A pile of things here.");
                }
                else if (itemsHere.Count() > 1)
                {
                    MessageLog.WriteLine("Some things here.");
                }
                else if (itemsHere.Count() == 1)
                {
                    var item = itemsHere.ElementAt(0);
                    var beVerb = item.Quantity == 1 ? "is" : "are";
                    var np = Describer.Describe(item, DescMods.Article);
                    MessageLog.WriteLine($"There {beVerb} {np} here.");
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
                MessageLog.WriteLine($"Picked up {item.Name}");
            }
            else
            {
                MessageLog.WriteLine($"Item {item.Name} was no longer on the map, to pick up");
            }
            return pickedUp;
        }

        int Time_spent_on_usable = 0;
        private bool Do_Usable(IBeing being, Command command)
        {
            if (command.Usable == null)
            {
                throw new Exception("Phlargieu!");
            }

            Time_spent_on_usable = 0;
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
                    var vp = command.Usable.VerbPhrase;
                    var np = Describer.Describe(command.Item, DescMods.Article);
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
            
                if (Time_spent_on_usable > 0)
                    ScheduleAgent(being, Time_spent_on_usable);
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
                Time_spent_on_usable += cost.Amount;
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
                string qualifier = space.CanTill ? "untilled " : "";
                MessageLog.WriteLine($"Cannot sow {qualifier}{space.Terrain.Name}.");
                return false;
            }

            if (space.IsSown)
            {
                MessageLog.WriteLine($"The ground to my {command.Direction} is already sown with a seed.");
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


        private bool Do_Wait(IBeing being, Command command)
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
