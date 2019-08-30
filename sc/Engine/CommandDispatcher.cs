﻿using System;
using System.Linq;
using log4net;
using SadConsole.Input;
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

        private Describer Describer;
        private IMessageLogWindow MessageLog;
        private ILog log;

        private Action<AsciiKey> NextStep = null;
        private bool InMultiStepCommand => NextStep != null;

        public CommandDispatcher(
            ISchedule schedule,
            IGameState gameState,
            Describer describer,
            IMessageLogWindow messageLog
        )
        {
            Schedule = schedule;
            GameState = gameState;
            Describer = describer;
            MessageLog = messageLog;
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
                throw new Exception("Unset action got into schedule somehow.");
            
            default:
                throw new Exception($"Need to write Dispatch() case for ScheduleAction.{nextAction.Action}.");
            }
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
                switch (fruit.PlantDetails.MainName)
                {
                case "Healer":
                    HealActor(being, 4);
                    FeedActor(being, 400);
                    break;

                default:
                    throw new Exception($"Don't have eating written for fruit of {fruit.PlantDetails.MainName}.");
                }

                var seed = new Seed((0, 0), 2, fruit.PlantDetails.ID);
                being.AddToInventory(seed);
                fruit.PlantDetails.FruitKnown = true;
                fruit.PlantDetails.SeedKnown = true;  //  Eating fruit also shows us what its seeds are.
                AddExperience(fruit.PlantDetails.ID, Exp.EatFruit);
                Schedule.AddAgent(being, 2);
            }

            //0.2  think about bail-out protocol on sanity check failure
            Guard.Against(item.Quantity < 1, $"Not enough {item.Name} to {item.ConsumeVerb}, somehow.");
            item.Quantity--;
            if (item.Quantity < 1)
                being.RemoveFromInventory(item);
            log.Info($"Consumed {item.Name} to no effect.  Needmorecode.");

            return true;
        }

        #region Direction
        private bool Do_Direction(IBeing being, CmdDirection direction)
        {
            var newPosition = CoordInDirection(being.Position, direction);

            IBeing targetBeing = BeingMap.GetItems(newPosition).FirstOrDefault();
            if (targetBeing != null)
            {
                return Do_DirectionAttack(being, targetBeing);
            }

            var blight = BlightMap.GetItem(newPosition);
            if (blight?.Extent > 0)
            {
                return Do_DirectionClearBlight(being, newPosition, blight);
            }

            return Do_DirectionMove(being, newPosition);
        }

        private bool Do_DirectionClearBlight(IBeing being, Coord newPosition, IAreaBlight targetBlight)
        {
            //0.2  wants smoother ux
            if (being.WieldedTool == null && being.Gloves == null)
            {
                Message(being, Msgs.BarehandBlightDamage);
                Damage(being, targetBlight);
                Damage(targetBlight, DamageType.Player, 6);
                GameState.Map.CoordsWithChanges.Add(newPosition);

                bool damageSpread = false;
                foreach (Coord neighbor in newPosition.Neighbors())
                {
                    IAreaBlight blight = BlightMap.GetItem(neighbor);
                    if (blight?.Extent > 0)
                    {
                        Damage(blight, DamageType.Player, 3);
                        GameState.Map.CoordsWithChanges.Add(neighbor);
                        damageSpread = true;
                    }
                }

                if (damageSpread)
                    Message(being, Msgs.BlightDamageSpreads);
            }
            else
            {
                Damage(targetBlight, being.WieldedTool);
                GameState.Map.CoordsWithChanges.Add(newPosition);
            }

            ScheduleAgent(being, 24);
            being.HasClearedBlightBefore = true;
            return true;
        }

        public enum DamageType
        {
            Unset = 0,
            AreaBlight,
            Player,
            Impact_point,
            Impact_edge,
            Impact_blunt,
        }

        public void Damage(IDestroyable target, IItem source)
        {
            int amount = DamageToTargetFromItem(target, source);
            Damage(target, amount);
        }

        public void Damage(IDestroyable target, IAreaBlight source)
        {
            int half = source.Extent * 5;

            int amount = half + new Random().Next(half) + 1;  //0.1.SAVE  need to use managed random

            Damage(target, DamageType.AreaBlight, amount);
        }

        public void Damage(IDestroyable target, IBeing source)
        {
            int amount = DamageToTargetFromBeing(target, source);
            Damage(target, amount);
        }

        // to collect modifiers to damage from type
        public void Damage(IDestroyable target, DamageType type, int amount)
        {
            if (type == DamageType.AreaBlight && target is Player)
            {
                //0.2.DMG  create more sophisticated player blight resistance
                amount = Math.Clamp(amount / 10, 1, 3);
            }
            //0.1.DMG  relocate more cases from elsewhere to here
            Damage(target, amount);
        }

        public void Damage(IDestroyable target, int amount)
        {
            target.Hurt(amount);
            if (target is AreaBlight blight)
            {
                if (blight.Extent == 0)
                {
                    GameState.Map.BlightMap.RemoveItem(blight);
                }
            }

            if (target.Health < 1)
            {
                //  Is this the afterlife?
                //  Some things 
                //TODO:  Remove?  Put on list to be
            }
        }

        private int DamageToTargetFromItem(IDestroyable target, IItem source)
        {
            throw new NotImplementedException();
        }

        //0.1.DMG  move beyond one special case and a couple of constants
        private int DamageToTargetFromBeing(IDestroyable target, IBeing source)
        {
            if (source.IsPlayer)
            {
                if (target is AreaBlight)
                {
                    return 6;
                }

                return 2;
            }

            return 4;
        }

        public enum Msgs
        {
            Unset = 0,
            BarehandBlightDamage,
            BlightDamageSpreads
        }

        public void Message(IBeing being, Msgs messageKey)
        {
            Guard.Against(messageKey == Msgs.Unset, "Must set message key");
            if (!being.IsPlayer) return;

            switch (messageKey)
            {
            case Msgs.BarehandBlightDamage:
                WriteLine("I tear it off the ground.  Burns... my hands start bleeding.  Acid?");
                if (!being.HasClearedBlightBefore)
                {
                    WriteLine("Where I touched it, the stuff is crumbling.");
                }
                break;

            case Msgs.BlightDamageSpreads:
                WriteLine("The damage to this stuff spreads outward.  Good.");
                break;

            default:
                throw new Exception($"Must code message for key [{messageKey}].");
            }
        }

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
                    GameState.Map.CoordsWithChanges.Add(newPosition);
                }
                else
                {
                    MessageLog.WriteLine("Door's stuck.");
                }
                return success;
            }

            if (!SpaceMap.CanWalkThrough(newPosition))
            {
                var np = Describer.Describe(space.Terrain.Name, DescMods.IndefiniteArticle);
                if (being.IsPlayer)
                    MessageLog.WriteLine($"I can't walk through {np}.");
                
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
                    var np = Describer.Describe(item, DescMods.IndefiniteArticle);
                    MessageLog.WriteLine($"There {beVerb} {np} here.");
                }
                else
                {
                } //  Nothing here, report nothing
            }

            return true;
        }

        private bool Do_DirectionAttack(IBeing being, IBeing target)
        {
            //0.1.DMG  instead decide damage and speed via attack & defense info
            target.Hurt(2);
            ScheduleAgent(being, 12);
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

            item.MoveTo(being.Position);
            ItemMap.Add(item, item.Location);
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
                MessageLog.WriteLine($"Picked up {item.Name}");
            }
            else
            {
                ScheduleAgent(being, 1);
                MessageLog.WriteLine($"Item {item.Name} was no longer on the map, to pick up");
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
            var targetCoord = CoordInDirection(being.Position, command.Direction);
            var space = SpaceMap.GetItem(targetCoord);
            if (space.IsTilled)
            {
                MessageLog.WriteLine("Ground here's already tilled.");
                return false;
            }

            if (!space.CanTill)
            {
                MessageLog.WriteLine($"Cannot till the {space.Terrain.Name}.");
                return false;
            }

            int tillTime = 15;
            if (being.WieldedTool != command.Item)
            {
                being.Wield(command.Item);
                tillTime += 6;
            }

            Till(space);
            GameState.Map.CoordsWithChanges.Add(targetCoord);
            ScheduleAgent(being, tillTime);
            return true;
        }

        private bool Use_Seed(IBeing being, Command command)
        {
            var targetCoord = CoordInDirection(being.Position, command.Direction);
            var space = SpaceMap.GetItem(targetCoord);

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

            var seedStock = (Seed)command.Item;
            var seedToSow = seedStock.GetSeedFromStack();
            SpaceMap.Sow(space, seedToSow);

            ScheduleAgent(seedToSow, 100);

            if (--seedStock.Quantity < 1)
            {
                being.RemoveFromInventory(seedStock);
            }

            AddExperience(seedToSow.PlantDetails.ID, Exp.PlantSeed);

            GameState.Map.CoordsWithChanges.Add(targetCoord);
            ScheduleAgent(being, 8);
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
