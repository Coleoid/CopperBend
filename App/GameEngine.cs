﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using CopperBend.App.Model;
using RLNET;

namespace CopperBend.App
{
    //  State of the game, not _quite_ globals...
    public interface IGameState
    {
        IAreaMap Map { get; }
        IActor Player { get; }
        GameMode Mode { get; set; }
    }

    public enum GameMode
    {
        Unknown = 0,
        MessagesPending,
        PlayerReady,
        Schedule
    }

    public class GameEngine : IGameState
    {
        private readonly RLRootConsole GameConsole;
        private readonly Scheduler Scheduler;
        private readonly Queue<RLKeyPress> InputQueue;

        public CommandDispatcher Dispatcher { get; }
        public IAreaMap Map { get; private set; }
        public IActor Player { get; private set; }

        public GameEngine(RLRootConsole console, Actor player)
        {
            GameConsole = console;
            Player = player;

            InputQueue = new Queue<RLKeyPress>();
            Scheduler = new Scheduler();
            Dispatcher = new CommandDispatcher(InputQueue, Scheduler);
        }

        public void LoadMap(IAreaMap map)
        {
            Map = map;
            foreach (var actor in map.Actors)
            {
                Scheduler.Add(new ScheduleEntry(12, actor));
            }

            map.Actors.Add(Player);
            map.UpdatePlayerFieldOfView(Player);
        }

        public void Run()
        {
            if (Player == null) throw new Exception("Must have Player before starting engine.");
            if (Map == null) throw new Exception("Must have Map before starting engine.");

            Dispatcher.Init(this);

            GameConsole.Update += onUpdate;
            GameConsole.Render += onRender;
            GameConsole.Run();
        }

        private void onRender(object sender, UpdateEventArgs e)
        {
            //FUTURE:  real-time (background) animation around here

            //  If the map hasn't changed, why render?
            if (!Map.DisplayDirty) return;

            GameConsole.Clear();
            Map.DrawMap(GameConsole);
            GameConsole.Draw();
            Map.DisplayDirty = false;
        }

        private void onUpdate(object sender, UpdateEventArgs e)
        {
            //  For now, only checking the keyboard for input
            RLKeyPress key = GameConsole.Keyboard.GetKeyPress();
            if (key != null)
            {
                if (key.Alt && key.Key == RLKey.F4)
                {
                    GameConsole.Close();
                    return;
                }

                InputQueue.Enqueue(key);
            }

            ActOnMode();
        }

        public GameMode Mode { get; set; }
        private void ActOnMode()
        {
            switch (Mode)
            {
            //FUTURE:  A game menu mode blocking all other action goes here
            //  Messages waiting for the player will stop anything else
            case GameMode.MessagesPending:
                Dispatcher.HandlePendingMessages();
                break;

            //  Waiting for player actions puts scheduled events on hold
            case GameMode.PlayerReady:
                Dispatcher.HandlePlayerCommands();
                break;

            //  When the player has committed to a slow action, everything happens
            case GameMode.Schedule:
                Scheduler.DoNext(this);
                break;

            case GameMode.Unknown:
                throw new Exception("Game mode unknown, perhaps Init() was missed.");

            default:
                throw new Exception($"Game mode [{Mode}] not written yet.");
            }
        }
    }
}
