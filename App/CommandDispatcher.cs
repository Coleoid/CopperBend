using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.App.Basis;
using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public partial class CommandDispatcher
    {
        private Queue<RLKeyPress> InputQueue;
        public Queue<string> MessageQueue { get; set; }
        public bool WaitingAtMorePrompt = false;

        public IActor Player { get; private set; }
        public IAreaMap Map { get; private set; }
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
            MessageQueue = new Queue<string>();
        }

        public void Init(IGameState gameState)
        {
            GameState = gameState;
            Player = gameState.Player;
            Map = gameState.Map;

            Message("I wake up.  Cold--frost on the ground, except where I was lying.");
            Message("Everything hurts when I stand up.");
            Message("The sky... says it's morning.  A small farmhouse to the east.");
            Message("Something real wrong with the ground to the west, and the north.");
        }

        #region Messages
        private int ShownMessages = 0;

        public void Message(string newMessage)
        {
            MessageQueue.Enqueue(newMessage);
            ShowMessages();
        }

        public void ShowMessages()
        {
            while (!WaitingAtMorePrompt && MessageQueue.Any())
            {
                if (ShownMessages >= 3)
                {
                    WriteLine("-- more --");
                    WaitingAtMorePrompt = true;
                    GameState.Mode = GameMode.MessagesPending;
                    return;
                }

                var nextMessage = MessageQueue.Dequeue();
                WriteLine(nextMessage);
                ShownMessages++;
            }
        }

        public void ClearMessagePanel()
        {
            //0.1
            ShownMessages = 0;
            WaitingAtMorePrompt = false;
        }

        public void HandlePendingMessages()
        {
            if (!WaitingAtMorePrompt) return;

            while (WaitingAtMorePrompt)
            {
                //  Advance to next space keypress, if any
                RLKeyPress key = null;
                while (InputQueue.Any() && key?.Key != RLKey.Space)
                {
                    key = InputQueue.Dequeue();
                }

                //  If we run out of keypresses before we find a space,
                // the rest of the messages remain pending
                if (key?.Key != RLKey.Space) return;

                //  Otherwise, show more messages
                ClearMessagePanel();
                ShowMessages();
            }

            //  If we reach this point, we cleared all messages
            GameState.Mode = IsPlayerScheduled ? 
                GameMode.Schedule : GameMode.PlayerReady;
        }
        #endregion

        public void HandlePlayerCommands()
        {
            if (!InputQueue.Any()) return;
            var key = InputQueue.Dequeue();

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
            else if (key.Key == RLKey.A)
            {
                Apply_Prompt(key);
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
            else if (key.Key == RLKey.W)
            {
                Wield_Prompt(key);
            }
            else if (key.Key == RLKey.Comma)
            {
                Command_PickUp();
            }

            //TODO: c, direction -> close door
            //TODO: l, direction, direction ... -> look around the map
            //TODO: l, ?, a-z -> look at inventory item
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
            if (Map.SetActorCoord(player, coord))
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
                ITile tile = Map[coord];
                if (tile.TerrainType == TerrainType.ClosedDoor)
                {
                    tile.OpenDoor();
                    Map.SetIsWalkable(tile, true);
                    Map.SetIsTransparent(tile, true);
                    Map.DisplayDirty = true;
                    Map.UpdatePlayerFieldOfView(player);
                    PlayerBusyFor(4);
                }
            }
        }
        private void Command_DirectionAttack(IActor targetActor, ICoord coord)
        {
            //0.1
            targetActor.Damage(2);
            Console.WriteLine("You hit the thingy for 2 points!");
            if (targetActor.Health < 1)
            {
                Console.WriteLine($"Blargh...  The {targetActor.Name} dies.");
                Map.Actors.Remove(targetActor);
                Map.SetIsWalkable(targetActor, true);
                Map.DisplayDirty = true;

                Scheduler.RemoveActor(targetActor);
            }

            PlayerBusyFor(12);
        }
        #endregion

        #region Apply Item
        private void Apply_Prompt(RLKeyPress key)
        {
            if (_usingItem == null) _usingItem = Player.WieldedTool;
            if (_usingItem == null)
            {
                Apply_Prompt_Choose(null);
                return;
            }

            Prompt($"Enter direction to apply {_usingItem.Name} (or ? to pick a different item): ");
            NextStep = Apply_in_Direction;
        }

        private void Apply_Prompt_Choose(RLKeyPress key)
        {
            Command_Inventory();
            Prompt("Pick an item to apply: ");
            NextStep = Apply_Choose_item;
        }

        private IItem _usingItem;
        private void Apply_in_Direction(RLKeyPress key)
        {
            if (key.Key == RLKey.Escape)
            {
                WriteLine("cancelled.");
                NextStep = null;
                return;
            }

            if (key.Key == RLKey.Slash && key.Shift)
            {
                Apply_Prompt_Choose(null);
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

        private void Apply_Choose_item(RLKeyPress key)
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
            NextStep = Apply_in_Direction;
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
