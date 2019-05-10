using System;
using System.Linq;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using GoRogue;
using log4net;
using Microsoft.Xna.Framework;
using SadConsole.Input;

namespace CopperBend.Engine
{
    public partial class CommandDispatcher
    {
        private ISchedule Schedule { get; set; }
        private IGameState GameState { get; set; }

        protected SpaceMap SpaceMap => GameState.Map.SpaceMap;
        private MultiSpatialMap<IBeing> BeingMap => GameState.Map.BeingMap;
        private MultiSpatialMap<IItem> ItemMap => GameState.Map.ItemMap;

        private Describer Describer;
        private IMessageOutput Output;
        private ILog log;

        private Action<AsciiKey> NextStep = null;
        private bool InMultiStepCommand => NextStep != null;

        public CommandDispatcher(
            ISchedule schedule,
            IGameState gameState,
            Describer describer,
            IMessageOutput messageOutput
        )
        {
            Schedule = schedule;
            GameState = gameState;
            Describer = describer;
            Output = messageOutput;
            log = LogManager.GetLogger("CB", "CB.Dispatcher");
        }

        public bool CommandBeing(IBeing being, Command command)
        {
            switch (command.Action)
            {
            case CmdAction.Consume:   return Do_Consume(being, command.Item);
            case CmdAction.Direction: return Do_Direction(being, command.Direction);
            case CmdAction.Drop:      return Do_Drop(being, command);
            case CmdAction.PickUp:    return Do_PickUp(being, command);
            case CmdAction.Use:       return Do_Use(being, command);
            case CmdAction.Wait:      return Do_Wait(being, command);
            case CmdAction.Wield:     return Do_Wield(being, command.Item);

            case CmdAction.Unknown:
            case CmdAction.Unset:
            case CmdAction.Incomplete:
            default:
                throw new Exception($"Not ready to do {command.Action}.");
            }
        }


        public bool Do_Consume(IBeing being, IItem item)
        {
            Guard.AgainstNullArgument(item, "No item in consume command");
            var invItem = being.RemoveFromInventory(item);
            Guard.AgainstNullArgument(invItem, "Item to consume not found in inventory");
            Guard.Against(!item.IsConsumable, "Item is not consumeable");

            if (item is Fruit fruit)
            {
                switch (fruit.PlantType)
                {
                case PlantType.Healer:
                    HealActor(being, 4);
                    FeedActor(being, 400);
                    break;

                default:
                    throw new Exception($"Don't have eating written for fruit of {fruit.PlantType}.");
                }

                being.AddToInventory(new Seed(new Point(0, 0), 2, fruit.PlantType));
                Learn(fruit);
                AddExperience(fruit.PlantType, Exp.EatFruit);
                Schedule.AddAgent(being, 2);
            }

            //0.1
            if (item.Quantity < 1)
                throw new Exception($"Not enough {item.Name} to {item.ConsumeVerb}, somehow.");
            item.Quantity--;
            if (item.Quantity < 1)
                being.RemoveFromInventory(item);
            log.Info($"Consumed {item.Name} to no effect.  Needmorecode.");

            return true;
        }

        #region Direction
        private bool Do_Direction(IBeing being, CmdDirection direction)
        {
            var point = PointInDirection(being.Location, direction);

            IBeing targetBeing = BeingMap.GetItems(point).FirstOrDefault();
            if (targetBeing == null)
            {
                return Do_DirectionMove(being, point);
            }

            return Do_DirectionAttack(being, targetBeing);
        }

        private bool Do_DirectionMove(IBeing being, Point offset)
        {
            var newPoint = being.Position + offset;
            Space tile = SpaceMap.GetItem(newPoint);

            if (tile.Terrain.Name == "closed door")
            {
                //TODO:  Map.OpenDoor(tile);
                ScheduleAgent(being, 4);
                return true;
            }

            if (!SpaceMap.CanWalkThrough(newPoint))
            {
                var np = Describer.Describe(tile.Terrain.Name, DescMods.IndefiniteArticle);
                if (being.IsPlayer)
                    Output.WriteLine($"I can't walk through {np}.");
                
                ClearPendingInput();
                return false;
            }

            being.Position += offset;
            PlayerMoved |= being.IsPlayer;

            int directionsMoved = 0;
            if (offset.X != 0) directionsMoved++;
            if (offset.Y != 0) directionsMoved++;
            if (directionsMoved == 0)
                throw new Exception("Moved nowhere?  Up/Down not yet handled.");
            else if (directionsMoved == 1)
                ScheduleAgent(being, 12);
            else
                ScheduleAgent(being, 17);

            if (being.IsPlayer)
            {
                var itemsHere = ItemMap.GetItems(newPoint);
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
                    var np = Describer.Describe(item, DescMods.IndefiniteArticle);
                    Output.WriteLine($"There {beVerb} {np} here.");
                }
                else
                {
                } //  Nothing here, report nothing
            }

            return true;
        }

        private bool Do_DirectionAttack(IBeing being, IBeing target)
        {
            //0.1
            target.Hurt(2);
            ScheduleAgent(being, 12);
            //0.2
            //var conflictSystem = new ConflictSystem(Window, Map, Schedule);
            //conflictSystem.Attack("Wah!", 2, targetActor);
            return true;
        }
        #endregion

        private bool Do_Drop(IBeing being, Command command)
        {
            Guard.AgainstNullArgument(command.Item, "No item in drop command");
            var item = being.RemoveFromInventory(command.Item);
            Guard.AgainstNullArgument(item, "Item to drop not found in inventory");

            item.MoveTo(being.Location);
            ItemMap.Add(item, item.Point);
            ScheduleAgent(being, 1);
            return true;
        }

        private bool Do_PickUp(IBeing being, Command command)
        {
            var item = command.Item;
            var pickedUp = ItemMap.Remove(item);
            if (pickedUp)
            {
                being.AddToInventory(item);
                ScheduleAgent(being, 4);
                Output.WriteLine($"Picked up {item.Name}");
            }
            else
            {
                ScheduleAgent(being, 1);
                Output.WriteLine($"Item {item.Name} was no longer on the map, to pick up");
            }
            return pickedUp;
        }

        private bool Do_Use(IBeing being, Command command)
        {
            switch (command.Item)
            {
            case Hoe h:
                return Use_Hoe(being, command);
            case Seed s:
                return Use_Seed(being, command);

            default:
                throw new Exception($"Don't know how to use a {command.Item.GetType().Name} yet.");
            }
        }

        private bool Use_Hoe(IBeing being, Command command)
        {
            var targetPoint = PointInDirection(being.Location, command.Direction);
            var space = SpaceMap.GetItem(targetPoint);
            if (space.CanPlant)
            {
                Output.WriteLine("All's ready to plant here, already.");
                return false;
            }

            if (!space.CanPreparePlanting)
            {

                Output.WriteLine($"Cannot till the {space.Terrain.Name}.");
                return false;
            }

            int tillTime = 15;
            if (being.WieldedTool != command.Item)
            {
                being.Wield(command.Item);
                tillTime += 6;
            }

            Till(space);
            ScheduleAgent(being, tillTime);
            return true;
        }

        private bool Use_Seed(IBeing being, Command command)
        {
            var targetPoint = PointInDirection(being.Location, command.Direction);
            var space = SpaceMap.GetItem(targetPoint);

            if (!space.IsTilled)
            {
                string qualifier = space.IsTillable ? "untilled " : "";
                Output.WriteLine($"Cannot sow {qualifier}{space.Terrain.Name}.");
                return false;
            }

            if (space.IsSown)
            {
                Output.WriteLine($"The ground to my {command.Direction} is already sown with a seed.");
                return false;
            }

            var seedStock = (Seed)command.Item;
            var seedToSow = seedStock.GetSeedFromStack();
            SpaceMap.Sow(space, seedToSow);

            ScheduleAgent(seedToSow, 100);

            if (--seedStock.Quantity < 1)
            {
                being.RemoveFromInventory(seedStock);
            }

            AddExperience(seedToSow.PlantType, Exp.PlantSeed);

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
