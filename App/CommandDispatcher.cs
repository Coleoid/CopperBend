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
        private Schedule Schedule { get; set; }

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
            log = LogManager.GetLogger("CB");
        }

        public bool CommandActor(IActor actor, Command command)
        {
            switch (command.Action)
            {
            case CmdAction.Consume:
                Do_Consume(actor, command.Item);
                break;

            case CmdAction.Direction:
                return Do_Direction(actor, command.Direction);

            case CmdAction.Drop:
                Do_Drop(actor, command);
                break;

            case CmdAction.PickUp:
                Do_PickUp(actor, command);
                break;

            case CmdAction.Use:
                Do_Use(actor, command);
                break;

            case CmdAction.Wait:
                Schedule.AddActor(actor, 6);
                break;

            case CmdAction.Wield:
                Do_Wield(actor, command.Item);
                break;

            case CmdAction.Unknown:
            case CmdAction.Unset:
            case CmdAction.Incomplete:
            default:
                throw new Exception($"Bad action {command.Action}.");
            }

            return false;
        }


        public void Do_Consume(IActor actor, IItem item)
        {
            Guard.Against(!item.IsConsumable);

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

                return;
            }

            //0.1
            if (item.Quantity < 1)
                throw new Exception($"Not enough {item.Name} to {item.ConsumeVerb}, somehow.");
            item.Quantity--;
            if (item.Quantity < 1)
                actor.RemoveFromInventory(item);
            log.Info($"Consumed {item.Name} to no effect.  Needmorecode.");
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

        private bool Do_DirectionMove(IActor actor, Point point)
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
                else
                {
                } //  Nothing here, report nothing
            }

            return true;
        }

        private bool Do_DirectionAttack(IActor actor, IActor target)
        {
            //0.1
            //var conflictSystem = new ConflictSystem(Window, Map, Schedule);
            //conflictSystem.Attack("Wah!", 2, targetActor);
            target.Hurt(2);
            ScheduleActor(actor, 12);
            return true;
        }
        #endregion

        private void Do_Drop(IActor actor, Command command)
        {
            command.Item.MoveTo(actor.Point);
            Map.Items.Add(command.Item);
            ScheduleActor(actor, 1);
        }

        private void Do_PickUp(IActor actor, Command command)
        {
            var topItem = Map.Items
                .Where(i => i.Point.Equals(actor.Point))
                .LastOrDefault();

            if (topItem == null)
            {
                Output.WriteLine("Nothing to pick up here.");
                return;
            }

            Map.Items.Remove(topItem);
            actor.AddToInventory(topItem);
            Output.WriteLine($"Picked up {topItem.Name}");
            ScheduleActor(actor, 4);
        }

        private void Do_Use(IActor actor, Command command)
        {
            var targetPoint = PointInDirection(actor.Point, command.Direction);
            command.Item.ApplyTo(Map[targetPoint], this, Output, command.Direction);
        }
        
        private void Do_Wield(IActor actor, IItem item)
        {
            actor.Wield(item);
            Output.WriteLine(item.Name);
            ScheduleActor(actor, 6);
        }
    }
}
