using System;
using System.Linq;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using log4net;
using RLNET;

namespace CopperBend.App
{
    public partial class CommandDispatcher
    {
        private ISchedule Schedule { get; set; }
        private IGameState GameState { get; set; }

        private IAreaMap Map
        {
            get => GameState.Map;
        }

        private Describer Describer;
        private EventBus EventBus;
        private IMessageOutput Output;
        private ILog log;

        private Action<RLKeyPress> NextStep = null;
        private bool InMultiStepCommand => NextStep != null;

        public CommandDispatcher(
            ISchedule schedule,
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
            log = LogManager.GetLogger("CB");
        }

        public bool CommandActor(IActor actor, Command command)
        {
            switch (command.Action)
            {
            case CmdAction.Consume:   return Do_Consume(actor, command.Item);
            case CmdAction.Direction: return Do_Direction(actor, command.Direction);
            case CmdAction.Drop:      return Do_Drop(actor, command);
            case CmdAction.PickUp:    return Do_PickUp(actor, command);
            case CmdAction.Use:       return Do_Use(actor, command);
            case CmdAction.Wait:      return Do_Wait(actor, command);
            case CmdAction.Wield:     return Do_Wield(actor, command.Item);

            case CmdAction.Unknown:
            case CmdAction.Unset:
            case CmdAction.Incomplete:
            default:
                throw new Exception($"Not ready to do {command.Action}.");
            }
        }


        public bool Do_Consume(IActor actor, IItem item)
        {
            Guard.AgainstNullArgument(item, "No item in consume command");
            var invItem = actor.RemoveFromInventory(item);
            Guard.AgainstNullArgument(invItem, "Item to consume not found in inventory");
            Guard.Against(!item.IsConsumable, "Item is not consumeable");

            if (item is Fruit fruit)
            {
                switch (fruit.PlantType)
                {
                case PlantType.Healer:
                    HealActor(actor, 4);
                    FeedActor(actor, 400);
                    break;

                default:
                    throw new Exception($"Don't have eating written for fruit of {fruit.PlantType}.");
                }

                actor.AddToInventory(new Seed(new Point(0, 0), 2, fruit.PlantType));
                Learn(fruit);
                Experience(fruit.PlantType, Exp.EatFruit);
                Schedule.AddActor(actor, 2);
            }

            //0.1
            if (item.Quantity < 1)
                throw new Exception($"Not enough {item.Name} to {item.ConsumeVerb}, somehow.");
            item.Quantity--;
            if (item.Quantity < 1)
                actor.RemoveFromInventory(item);
            log.Info($"Consumed {item.Name} to no effect.  Needmorecode.");

            return true;
        }

        #region Direction
        private bool Do_Direction(IActor actor, CmdDirection direction)
        {
            var point = PointInDirection(actor.Point, direction);

            IActor targetActor = Map.GetActorAtPoint(point);
            if (targetActor == null)
            {
                return Do_DirectionMove(actor, point);
            }

            return Do_DirectionAttack(actor, targetActor);
        }

        private bool Do_DirectionMove(IActor actor, Point newPoint)
        {
            ITile tile = Map[newPoint];
            if (tile.TileType.Name == "closed door")
            {
                Map.OpenDoor(tile);
                ScheduleActor(actor, 4);
                return true;
            }

            if (!Map.IsWalkable(newPoint))
            {
                var np = Describer.Describe(tile.TileType.Name, DescMods.IndefiniteArticle);
                if (actor.IsPlayer)
                    Output.WriteLine($"I can't walk through {np}.");
                EventBus.ClearPendingInput(this, new EventArgs());
                return false;
            }

            CheckActorAtCoordEvent(actor, tile);

            var startingPoint = actor.Point;
            Map.MoveActor(actor, newPoint);
            Map.UpdatePlayerFieldOfView(actor);
            Map.IsDisplayDirty = true;

            int directionsMoved = 0;
            if (actor.Point.X != startingPoint.X) directionsMoved++;
            if (actor.Point.Y != startingPoint.Y) directionsMoved++;
            if (directionsMoved == 0)
                throw new Exception("Moved nowhere?  Up/Down not yet handled.");
            else if (directionsMoved == 1)
                ScheduleActor(actor, 12);
            else
                ScheduleActor(actor, 17);

            if (actor.IsPlayer)
            {
                var itemsHere = Map.Items.Where(i => i.Point == newPoint);
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

        private bool Do_DirectionAttack(IActor actor, IActor target)
        {
            //0.1
            target.Hurt(2);
            ScheduleActor(actor, 12);
            //0.2
            //var conflictSystem = new ConflictSystem(Window, Map, Schedule);
            //conflictSystem.Attack("Wah!", 2, targetActor);
            return true;
        }
        #endregion

        private bool Do_Drop(IActor actor, Command command)
        {
            Guard.AgainstNullArgument(command.Item, "No item in drop command");
            var item = actor.RemoveFromInventory(command.Item);
            Guard.AgainstNullArgument(item, "Item to drop not found in inventory");

            item.MoveTo(actor.Point);
            Map.Items.Add(item);
            ScheduleActor(actor, 1);
            return true;
        }

        private bool Do_PickUp(IActor actor, Command command)
        {
            var topItem = Map.Items
                .Where(i => i.Point.Equals(actor.Point))
                .LastOrDefault();

            if (topItem == null)
            {
                Output.WriteLine("Nothing to pick up here.");
                return false;
            }

            Map.Items.Remove(topItem);
            actor.AddToInventory(topItem);
            Output.WriteLine($"Picked up {topItem.Name}");
            ScheduleActor(actor, 4);
            return true;
        }

        private bool Do_Use(IActor actor, Command command)
        {
            var targetPoint = PointInDirection(actor.Point, command.Direction);
            command.Item.ApplyTo(Map[targetPoint], this, Output, command.Direction);
            return true;
        }

        private bool Do_Wait(IActor actor, Command command)
        {
            Schedule.AddActor(actor, 6);
            return true;
        }

        private bool Do_Wield(IActor actor, IItem item)
        {
            actor.Wield(item);
            Output.WriteLine(item.Name);
            ScheduleActor(actor, 6);
            return true;
        }
    }
}
