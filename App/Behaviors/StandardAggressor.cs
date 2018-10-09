using System;
using System.Linq;
using CopperBend.App.Basis;

namespace CopperBend.App.Behaviors
{
    public interface IBehavior
    {
        ScheduleEntry NextAction(ScheduleEntry entry, IControlPanel controls);
    }

    public class StandardMoveAndAttack : IBehavior
    {
        private const int ChaseTurnsLimit = 15;
        private int TurnsTargetOutOfFOV = ChaseTurnsLimit + 1;
        private bool IsAlerted => TurnsTargetOutOfFOV < ChaseTurnsLimit;

        public ScheduleEntry NextAction(ScheduleEntry entry, IControlPanel controls)
        {
            var actor = entry.Actor;
            bool isInFOV = controls.CanActorSeeTarget(actor, controls.PlayerCoords);

            if (isInFOV)
            {
                if (!IsAlerted) controls.WriteLine($"The {actor.Name} spots me.");

                TurnsTargetOutOfFOV = 0;
            }

            var ticks = AttemptMoveAttack(actor, controls);  //  the magic

            //  Fall asleep if chased for 15 turns w/no glimpse
            TurnsTargetOutOfFOV++;
            Func<ScheduleEntry, IControlPanel, ScheduleEntry> action = NextAction;
            if (!IsAlerted) action = Sleep;

            return new ScheduleEntry(ticks, action);
        }

        public ScheduleEntry Sleep(ScheduleEntry entry, IControlPanel controls)
        {
            //TODO: add some sensory checks--currently more of a coma.
            return entry;
        }

        private static int AttemptMoveAttack(IActor actor, IControlPanel controls)
        {
            var target = controls.PlayerCoords;
            var pathList = controls.GetPathTo(actor, target);

            // player in FOV, but not reachable
            if (!pathList.Any())
            {
                Console.WriteLine($"The {actor.Name} waits for an opening...");
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
                return 30;
            }

            throw new Exception("Why am I here?");
        }
    }
}
