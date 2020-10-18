using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CopperBend.Contract;
using GoRogue;

namespace CopperBend.Logic
{
    public class TriggerPuller : ITriggerPuller
    {
        [InjectProperty] public IControlPanel ControlPanel { get; set; }
        [InjectProperty] public IGameState GameState { get; set; }
        [InjectProperty] public IMessager Messager { get; set; }


        private Dictionary<Trigger, ITriggerHolder> TriggerHeldBy { get; set; }

        public IEnumerable<Trigger> TriggersInScope
        {
            get => TriggerHeldBy.Keys;
        }

        public TriggerPuller()
        {
            TriggerHeldBy = new Dictionary<Trigger, ITriggerHolder>();
        }

        public void AddTriggerHolderToScope(ITriggerHolder holder)
        {
            foreach (var trig in holder.ListTriggers())
            {
                TriggerHeldBy[trig] = holder;
            }
        }

        public void RemoveTriggerHolderFromScope(ITriggerHolder holder)
        {
            foreach (var trig in holder.ListTriggers())
            {
                TriggerHeldBy.Remove(trig);
            }
        }

        public void Check(TriggerCategories categories)
        {
            // Has any of the categories.  .HasFlag() means 'has all the flags' instead.
            var trigs = TriggersInScope
                .Where(t => (t.Categories & categories) > TriggerCategories.Unset)
                .ToList();

            foreach (var trig in trigs)
            {
                if (ConditionMet(trig))
                    ExecuteScript(trig);
            }
        }

        public bool ConditionMet(Trigger trigger)
        {
            // This implementation has some headroom,
            // though not enough for the long term.  Proper parsing later.
            Match match;

            // repeat this combo for each condition pattern.
            match = Regex.Match(
                trigger.Condition,
                @" *\( *(\d+), *(\d+) *\)( to +\( *(\d+), *(\d+) *)?"
            );
            if (match.Success) return ConditionMet_Location(match);

            // et c...

            throw new Exception($"Condition [{trigger.Condition}] of trigger [{trigger.Name}] didn't match any known pattern.");
        }


        // --------------------------- sorta workin', below.

        private bool ConditionMet_Location(Match match)
        {
            Coord location = GameState.Player.GetPosition();

            int x = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            int y = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

            bool isSingleCoord = string.IsNullOrEmpty(match.Groups[3].Value);
            if (isSingleCoord) return location.X == x && location.Y == y;

            int toX = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
            int toY = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);

            return x <= location.X && location.X <= toX
                && y <= location.Y && location.Y <= toY;
        }

        protected internal List<Trigger> GetLocationTriggers() =>
            TriggersInScope
            .Where(t => t.Categories.HasFlag(TriggerCategories.PlayerLocation))
            .ToList();

        //  Seems sensible that the engine points which trigger different categories
        // will be in different places, and know which sort of triggers might fire,
        // so baking category into the API should work?
        public void CheckPlayerLocationTriggers(Coord playerLocation)
        {
            foreach (var trigger in GetLocationTriggers())
            {
                if (LocationShouldFire(playerLocation, trigger))
                    ExecuteScript(trigger);
            }
        }

        public bool LocationShouldFire(Coord location, Trigger locationTrigger)
        {
            var match = Regex.Match(
                locationTrigger.Condition,
                @"^ *\( *(\d+), *(\d+) *\)( to +\( *(\d+), *(\d+) *)?"
            );

            if (!match.Success)
                throw new FormatException($"Location Trigger doesn't understand the condition [{locationTrigger.Condition}].");

            int x = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            int y = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

            bool isSingleCoord = string.IsNullOrEmpty(match.Groups[3].Value);
            if (isSingleCoord) return location.X == x && location.Y == y;

            int toX = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
            int toY = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);

            return x <= location.X && location.X <= toX
                && y <= location.Y && location.Y <= toY;
        }


        private void ExecuteScript(Trigger trigger)
        {
            var script = trigger.Script;
            var length = script.Count;
            for (int i = 0; i < length; i++)
            {
                var line = script[i];

                if (line == "message")
                {
                    int end = 0;
                    int j = i + 1;
                    for (; j < length; j++)
                    {
                        if (script[j] == "end message")
                        {
                            end = j;
                            break;
                        }
                    }
                    if (end == 0)
                        throw new Exception($"No end to message starting on script line {i} of trigger {trigger.Name}.");
                    Message(script.GetRange(i + 1, j - i - 1));
                    i = j + 1;

                    continue;
                }

                if (line == "remove trigger")
                {
                    RemoveTrigger(trigger);
                    continue;
                }

                if (line.StartsWith("move player"))
                {
                    var match = Regex.Match(
                        line,
                        @"move player +\( *(\d+), *(\d+) *\)",
                        RegexOptions.IgnoreCase
                    );
                    if (!match.Success)
                        throw new FormatException($"Move player doesn't find location in [{line}].");

                    int x = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    int y = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

                    GameState.Player.MoveTo((x, y));
                    continue;
                }

                if (line == "")
                {
                    continue;
                }

                throw new Exception($"Don't know script command [{line}].");
            }
            // message, scroll or pop-up
            // change map
            // set player location
            // delete trigger
        }

        private void RemoveTrigger(Trigger trigger)
        {
            TriggerHeldBy[trigger].RemoveTrigger(trigger);
            TriggerHeldBy.Remove(trigger);
        }

        private void Message(List<string> lines)
        {
            foreach (var line in lines) Messager.WriteLine(line);
        }
    }
}
