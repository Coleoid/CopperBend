﻿using System;
using System.Collections.Generic;
using System.Linq;
using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public partial class CommandDispatcher
    {
        private Queue<RLKeyPress> InputQueue;

        public IActor Player { get => GameState.Player; }
        public IAreaMap Map { get => GameState.Map; }
        public Scheduler Scheduler { get; private set; }
        public IGameState GameState { get; private set; }

        private Action<RLKeyPress> NextStep = null;
        private bool InMultiStepCommand
        {
            get => NextStep != null;
        }

        public CommandDispatcher(Queue<RLKeyPress> inputQueue, Scheduler scheduler)
        {
            InputQueue = inputQueue;
            Scheduler = scheduler;
        }

        public void Init(IGameState gameState)
        {
            GameState = gameState;
        }


        public void HandlePlayerCommands()
        {
            var key = GetNextKeyPress();
            if (key == null) return;

            if (InMultiStepCommand)  //  Drop, throw, wield, etc.
            {
                NextStep(key);
                return;
            }

            var direction = DirectionOfKey(key);
            if (direction != Direction.None)
            {
                Command_Direction(Player, direction);
            }
            else if (key.Key == RLKey.C)
            {
                Consume_Prompt(key);
            }
            else if (key.Key == RLKey.D)
            {
                Drop_Prompt(key);
            }
            else if (key.Key == RLKey.H || key.Key == RLKey.Slash && key.Shift)
            {
                Command_Help();
            }
            else if (key.Key == RLKey.I)
            {
                Command_Inventory();
            }
            else if (key.Key == RLKey.U)
            {
                Use_Prompt(key);
            }
            else if (key.Key == RLKey.W)
            {
                Wield_Prompt(key);
            }
            else if (key.Key == RLKey.Comma)
            {
                Command_PickUp();
            }

            //TODO: close door
            //TODO: [l, direction, direction, ...] -> look around the map
            //TODO: [l, ?, a-z] -> look at inventory item
            //TODO: ...
        }

        #region Direction
        private void Command_Direction(IActor player, Direction direction)
        {
            var coord = newCoord(player, direction);

            IActor targetActor = Map.GetActorAtCoord(coord);
            if (targetActor == null)
            {
                Command_DirectionMove(player, coord);
            }
            else
            {
                Command_DirectionAttack(targetActor, coord);
            }
        }
        private void Command_DirectionMove(IActor player, ICoord coord)
        {
            //  If we actually do move in that direction,
            //  we need to redraw, and the player will be busy for a while.
            ITile tile = Map[coord];
            if (tile.TileType.Name == "ClosedDoor")
            {
                Map.OpenDoor(tile);
                PlayerBusyFor(4);
            }
            else if (Map.HasEventAtCoords(tile))
            {
                Map.RunEvent(player, tile, this);
            }
            else if (Map.SetActorCoord(player, coord))
            {
                Map.UpdatePlayerFieldOfView(player);
                Map.DisplayDirty = true;
                if (player.X != coord.X && player.Y != coord.Y)
                    PlayerBusyFor(17);
                else
                    PlayerBusyFor(12);
            }
            else
            {
                WriteLine($"I can't walk through {tile.TileType.Name}.");
            }
        }

        private void Command_DirectionAttack(IActor targetActor, ICoord coord)
        {
            //0.1
            int damage = 2;
            targetActor.AdjustHealth(-damage);
            Console.WriteLine($"I hit the {targetActor.Name} for {damage}.");
            if (targetActor.Health < 1)
            {
                Console.WriteLine($"The {targetActor.Name} dies.");
                Map.Actors.Remove(targetActor);
                Map.SetIsWalkable(targetActor, true);
                Map.DisplayDirty = true;

                //TODO: drop items, body

                Scheduler.RemoveActor(targetActor);
            }

            PlayerBusyFor(12);
        }
        #endregion

        #region Consume
        private void Consume_Prompt(RLKeyPress key)
        {
            Prompt("Consume (inventory letter or ? to show inventory): ");
            NextStep = Consume_Main;
        }

        private void Consume_Main(RLKeyPress key)
        {
            //  Bail out
            if (key.Key == RLKey.Escape)
            {
                WriteLine("nothing.");
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
                WriteLine($"I can't {item.ConsumeVerb} a {item.Name}.");
                Consume_Prompt(null);
                return;
            }

            item.Consumed((IControlPanel)this);

            NextStep = null;
        }
        #endregion

        #region Drop

        private void Drop_Prompt(RLKeyPress key)
        {
            Prompt("Drop (inventory letter or ? to show inventory): ");
            NextStep = Drop_Main;

        }

        private void Drop_Main(RLKeyPress key)
        {
            //  Bail out
            if (key.Key == RLKey.Escape)
            {
                WriteLine("nothing.");
                NextStep = null;
                return;
            }

            //  Show inventory, re-prompt, wait for selection
            if (key.Key == RLKey.Slash && key.Shift)
            {
                Command_Inventory();
                Drop_Prompt(null);
                return;
            }

            int inventorySlot = AlphaIndexOfKeyPress(key);
            if (inventorySlot == -1) return;

            var wieldedItem = Player.WieldedTool;
            IItem item = Player.RemoveFromInventory(inventorySlot);

            if (item == null)
            {
                WriteLine($"No item labelled '{key.Char.Value}'.");
                Drop_Prompt(null);
                return;
            }

            WriteLine(item.Name);
            if (wieldedItem == item)
                WriteLine($"Note:  No longer wielding the {item.Name}.");

            item.MoveTo(Player.X, Player.Y);
            Map.Items.Add(item);
            PlayerBusyFor(1);

            NextStep = null;
        }
        #endregion

        private void Command_Help()
        {
            WriteLine("Help:");
            WriteLine("Arrow or numpad keys to move and attack");
            WriteLine("a)pply wielded tool");
            WriteLine("d)rop an item");
            WriteLine("h)elp (or ?) shows this message");
            WriteLine("i)nventory");
            WriteLine("w)ield a tool");
            WriteLine(",) Pick up object");
        }

        private void Command_Inventory()
        {
            var watcher = new ItemWatcher();

            Console.WriteLine("Inventory:");
            if (Player.Inventory.Count() == 0)
            {
                Console.WriteLine("empty.");
            }
            else
            {
                int asciiSlot = 97;
                foreach (var item in Player.Inventory)
                {
                    var description = watcher.Describe(item);
                    Console.WriteLine($"{(char)asciiSlot})  {description}");
                    asciiSlot++;
                }
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

            Prompt($"Direction key to use {_usingItem.Name}, or pick another item with a-z or ?: ");
            NextStep = Use_in_Direction;
        }

        private void Use_Prompt_Choose(RLKeyPress key)
        {
            Command_Inventory();
            Prompt("Pick an item to use: ");
            NextStep = Use_Choose_item;
        }

        private IItem _usingItem;
        private void Use_in_Direction(RLKeyPress key)
        {
            if (key.Key == RLKey.Escape)
            {
                WriteLine("cancelled.");
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
                var targetCoord = newCoord(Player, direction);
                WriteLine(direction.ToString());

                _usingItem.ApplyTo(Map[targetCoord], this);  // the magic
                NextStep = null;
            }
        }

        private void Use_Choose_item(RLKeyPress key)
        {
            if (key.Key == RLKey.Escape)
            {
                Console.WriteLine("cancelled.");
                NextStep = null;
                return;
            }

            var selectedIndex = AlphaIndexOfKeyPress(key);
            if (selectedIndex < 0 || selectedIndex > Player.Inventory.Count())
            {
                WriteLine($"The key [{key.Char}] does not match an inventory item.  Pick another.");
                return;
            }

            var item = Player.Inventory.ElementAt(selectedIndex);
            if (!item.IsUsable)
            {
                WriteLine($"The [{item.Name}] is not a usable item.  Pick another.");
                return;
            }

            _usingItem = item;
            WriteLine($"Using {item.Name} in what direction: ");
            NextStep = Use_in_Direction;
        }
        #endregion

        #region Wield
        private void Wield_Prompt(RLKeyPress key)
        {
            Prompt("Wield ('?' to show inventory, '.' to empty hands): ");
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
                WriteLine($"unchanged, {name}.");
                NextStep = null;
                return;
            }

            //  Period:  Wield empty hands, done
            if (key.Key == RLKey.Period)
            {
                WriteLine("bare hands");
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
                WriteLine($"nothing in slot '{key.Char.Value}'.");
                Wield_Prompt(null);
                return;
            }

            // we can wield even silly stuff, until it's a problem.
            var item = Player.Inventory.ElementAt(inventorySlot);

            Player.Wield(item);
            WriteLine(item.Name);
            PlayerBusyFor(4);
            NextStep = null;
        }
        #endregion

        private void Command_PickUp()
        {
            var topItem = Map.Items
                .Where(i => i.X == Player.X && i.Y == Player.Y)
                .LastOrDefault();

            if (topItem == null)
            {
                Console.WriteLine("Nothing to pick up here.");
                return;
            }

            Map.Items.Remove(topItem);
            Player.AddToInventory(topItem);
            Console.WriteLine($"Picked up {topItem.Name}");
            PlayerBusyFor(2);
        }

    }
}
