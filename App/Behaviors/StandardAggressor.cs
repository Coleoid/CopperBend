using System;
using RogueSharp;
using System.Linq;

namespace CopperBend.App.Behaviors
{
    public interface IBehavior
    {
        ScheduleEntry Act(ScheduleEntry entry, IGameState state);
    }

    public class StandardMoveAndAttack : IBehavior
    {
        private bool IsAlerted = false;
        private int TurnsAlerted = 0;

        public ScheduleEntry Act(ScheduleEntry entry, IGameState state)
        {
            var actor = entry.Actor;

            FieldOfView monsterFov = new FieldOfView(state.Map);
            monsterFov.ComputeFov(actor.X, actor.Y, actor.Awareness, true);
            bool isInFOV = monsterFov.IsInFov(state.Player.X, state.Player.Y);
            
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
                    AttemptMoveAttack(actor, state.Map, state.Player);
                }
                else
                {
                    //0.1:  upgrade = pursue existing path
                    TurnsAlerted++;
                }

                // Lose alert if the player is out of FOV for 15 turns. 
                IsAlerted = TurnsAlerted <= 15;
            }

            //TODO:  next move delayed depending on action taken
            // entry.TicksUntilNextAction = isDiagMove ? 17 : 12;

            return entry;
        }

        private static int AttemptMoveAttack(IActor actor, IAreaMap map, IActor target)
        {
            // Pathfinder needs the origin and target Cells walkable
            map.SetIsWalkable(actor, true);
            map.SetIsWalkable(target, true);

            PathFinder pathFinder = new PathFinder(map, 1.0, Math.Sqrt(2));

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
                        return 12;
                    }

                    return 6;
                }
                else
                {
                    bool isDiag = actor.X != cell.X && actor.Y != cell.Y;
                    return isDiag ? 17 : 12;
                }
            }
            else
            {
                // player in FOV, but not reachable
                Console.WriteLine($"{actor.Name} waits...");
                return 6;
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
