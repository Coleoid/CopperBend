using System;
using System.Linq;

namespace CopperBend.App.Behaviors
{
    public interface IBehavior
    {
        void NextAction(IControlPanel controls, ScheduleEntry entry);
    }

    public class StandardMoveAndAttack : IBehavior
    {
        private const int ChaseTurnsLimit = 15;
        private int TurnsTargetOutOfFOV = ChaseTurnsLimit + 1;
        private bool IsAlerted => TurnsTargetOutOfFOV < ChaseTurnsLimit;

        public void NextAction(IControlPanel controls, ScheduleEntry entry)
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
            Action<IControlPanel, ScheduleEntry> action = NextAction;
            if (!IsAlerted) action = Sleep;

            controls.AddToSchedule(new ScheduleEntry(ticks, action, actor));
        }

        public void Sleep(IControlPanel controls, ScheduleEntry entry)
        {
            //TODO: add some sensory checks--currently more of a coma than a nap.
            controls.AddToSchedule(entry);
        }

        private static int AttemptMoveAttack(IActor actor, IControlPanel controls)
        {
            var target = controls.PlayerCoords;
            var pathList = controls.GetPathTo(actor.Coord, target);

            // player in FOV, but not reachable
            if (!pathList.Any())
            {
                Console.WriteLine($"The {actor.Name} waits for an opening...");
                return 6;
            }

            // Take the step.  The only reason to fail right now is being blocked by the player.
            var step = pathList.First();
            bool isDiag = actor.Coord.X != step.X && actor.Coord.Y != step.Y;
            if (controls.MoveActorTo(actor, step))
            {
                controls.SetMapDirty();
                return isDiag ? 17 : 12;
            }

            if (target.X == step.X && target.Y == step.Y)
            {
                controls.AttackPlayer(actor);
                return 30;
            }

            throw new Exception("Why am I here?");
        }
    }
}
