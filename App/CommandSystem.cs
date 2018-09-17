using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperBend.App
{
    public class CommandSystem
    {
        // Return value is true if the player was able to move
        // false when the player couldn't move, such as trying to move into a wall
        public bool MoveActor(Actor actor, Direction direction, IcbMap map)
        {
            int x = actor.X;
            int y = actor.Y;

            switch (direction)
            {
                case Direction.Up:
                {
                    y = actor.Y - 1;
                    break;
                }
                case Direction.Down:
                {
                    y = actor.Y + 1;
                    break;
                }
                case Direction.Left:
                {
                    x = actor.X - 1;
                    break;
                }
                case Direction.Right:
                {
                    x = actor.X + 1;
                    break;
                }
                default:
                {
                    return false;
                }
            }

            return map.SetActorPosition(actor, x, y);
        }
    }
}
