using GoRogue;

namespace CopperBend.Model
{
    public class Hoe : Item
    {
        public override string Name { get => "Hoe"; }
        public override string ItemType { get => "Hoe"; }

        public Hoe(Coord position, uint id = uint.MaxValue)
            : base(position, 1, id)
        {
            Glyph = '\\';
        }

        //public override void ApplyTo(Space space, IControlPanel controls, IMessageOutput output, CmdDirection direction)
        //{
        //    if (space.IsTillable)
        //    {
        //        if (space.IsTilled)
        //        {
        //            output.WriteLine("Already tilled.");
        //            return;
        //        }

        //        controls.Till(space);
        //        //controls.SetMapDirty();
        //        //controls.ScheduleActor(15);
        //    }
        //    else
        //    {
        //        output.WriteLine($"Cannot till the {space.Terrain.Name}.");
        //    }
        //}
    }
}
