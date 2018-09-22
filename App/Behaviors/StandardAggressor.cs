﻿using System;
using RogueSharp;
using System.Linq;

namespace CopperBend.App.Behaviors
{
    public interface IBehavior
    {
        ScheduleEntry Act(ScheduleEntry entry, IAreaMap map, IActor player);
    }

    public class StandardMoveAndAttack : IBehavior
    {
        private bool IsAlerted = false;
        private int TurnsAlerted = 0;

        public ScheduleEntry Act(ScheduleEntry entry, IAreaMap map, IActor player)
        {
            var actor = entry.Actor;

            FieldOfView monsterFov = new FieldOfView(map);
            monsterFov.ComputeFov(actor.X, actor.Y, actor.Awareness, true);
            bool isInFOV = monsterFov.IsInFov(player.X, player.Y);
            
            if (!IsAlerted)
            {
                if (isInFOV)
                {
                    Console.WriteLine($"{actor.Name} notices you.");
                    TurnsAlerted = 1;
                    IsAlerted = true;
                }
            }

            if (IsAlerted)
            {
                if (isInFOV)
                {
                    TurnsAlerted = 1;
                    AttemptMoveAttack(actor, map, player);
                }
                else
                {
                    //0.1:  upgrade = pursue existing path
                    TurnsAlerted++;
                }

                // Lose alert if the player is out of FOV for 15 turns. 
                IsAlerted = TurnsAlerted <= 15;
            }

            return entry;
        }

        private static void AttemptMoveAttack(IActor actor, IAreaMap map, IActor target)
        {
            // Pathfinder needs the origin and target Cells walkable
            map.SetIsWalkable(actor, true);
            map.SetIsWalkable(target, true);

            PathFinder pathFinder = new PathFinder(map);

            var pathList = pathFinder.ShortestPathList(
                map.GetCell(actor.X, actor.Y),
                map.GetCell(target.X, target.Y));

            map.SetIsWalkable(actor, false);
            map.SetIsWalkable(target, false);


            // Take the step.  If blocked by our target, attack.
            if (pathList.Any())
            {
                var cell = pathList.First();
                if (!map.SetActorPosition(actor, cell.X, cell.Y))
                {
                    if (target.X == cell.X && target.Y == cell.Y)
                    {
                        AttackPlayer(target);
                    }
                }
            }
            else
            {
                // player in FOV, but not reachable
                Console.WriteLine($"{actor.Name} waits...");
            }
        }

        private static void AttackPlayer(IActor player)
        {
            player.Damage(2);
            Console.WriteLine("the thingy hit you for 2 points!");
            if (player.Health < 1)
            {
                Console.WriteLine("You die...");
                //TODO: die
            }
        }
    }
}