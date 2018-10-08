using System;
using System.Linq;
using CopperBend.App.Basis;

namespace CopperBend.App.Behaviors
{
    public interface IBehavior
    {
        ScheduleEntry Act(ScheduleEntry entry, IControlPanel controls);
    }

    public class StandardMoveAndAttack : IBehavior
    {
        private bool IsAlerted = false;
        private int TurnsAlerted = 0;

        public ScheduleEntry Act(ScheduleEntry entry, IControlPanel controls)
        {
            var actor = entry.Actor;
            bool isInFOV = controls.CanActorSeeTarget(actor, controls.PlayerCoords);

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
                    var ticks = AttemptMoveAttack(actor, controls);
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

        private static int AttemptMoveAttack(IActor actor, IControlPanel controls)
        {
            var target = controls.PlayerCoords;
            var pathList = controls.GetPathTo(actor, target);

            // player in FOV, but not reachable
            if (!pathList.Any())
            {
                Console.WriteLine($"{actor.Name} waits...");
                return 6;
            }

            // Take the step.  The only reason to fail right now is being blocked by the player.
            var step = pathList.First();
            bool isDiag = actor.X != step.X && actor.Y != step.Y;
            if (controls.MoveActorTo(actor, step))
            {
                controls.SetMapDirty();
                return isDiag ? 17 : 12;
            }

            if (target.X == step.X && target.Y == step.Y)
            {
                controls.AttackPlayer();
                return 12;
            }

            throw new Exception("Why am I here?");
        }
    }
}
