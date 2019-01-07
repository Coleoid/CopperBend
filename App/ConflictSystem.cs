namespace CopperBend.App
{
    public class ConflictSystem
    {
        public GameWindow GameWindow { get; private set; }
        public void WriteLine(string text) => GameWindow.WriteLine(text);

        public IAreaMap Map { get; private set; }
        public Scheduler Scheduler { get; private set; }

        public ConflictSystem(GameWindow gameWindow, IAreaMap map, Scheduler scheduler)
        {
            GameWindow = gameWindow;
            Map = map;
            Scheduler = scheduler;
        }

        public void Attack(string attack, int damage, IHealAndHurt target)
        {
            target.Hurt(damage);
            WriteLine($"I hit the {target.Entity.Name} for {damage}.");
            if (target.Health < 1)
            {
                WriteLine($"The {target.Entity.Name} dies.");
                Map.Actors.Remove(target.Entity);
                Map.SetIsWalkable(target.Entity.Point, true);
                Map.DisplayDirty = true;

                //TODO: drop items, body

                Scheduler.RemoveActor(target.Entity);
            }
        }
    }
}
