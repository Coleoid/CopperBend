namespace CopperBend.App
{
    public class ConflictSystem
    {
        public Messenger Messenger { get; private set; }
        public void WriteLine(string text) => Messenger.WriteLine(text);

        public IAreaMap Map { get; private set; }
        public Scheduler Scheduler { get; private set; }

        public ConflictSystem(Messenger messenger, IAreaMap map, Scheduler scheduler)
        {
            Messenger = messenger;
            Map = map;
            Scheduler = scheduler;
        }

        public void Attack(string attack, int damage, IActor target)
        {
            target.AdjustHealth(-damage);
            WriteLine($"I hit the {target.Name} for {damage}.");
            if (target.Health < 1)
            {
                WriteLine($"The {target.Name} dies.");
                Map.Actors.Remove(target);
                Map.SetIsWalkable(target.Point, true);
                Map.DisplayDirty = true;

                //TODO: drop items, body

                Scheduler.RemoveActor(target);
            }
        }
    }
}
