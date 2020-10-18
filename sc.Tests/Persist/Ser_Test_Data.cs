using CopperBend.Contract;
using System.Collections.Generic;

namespace CopperBend.Creation.Tests
{
    public class Ser_Test_Data
    {
        public static string Get_RR_Yaml()
        {
            return @"!CompoundMapBridge
name: Round Room
width: 7
height: 5
legend:
  .: soil
  '#': stone wall
  +: closed door
terrain:
- ..###..
- .##.##.
- .#...+.
- .##.##.
- ..###..
areaRots: {}
multiBeings: {}
multiItems: {}
triggers:
- !Trigger
  name: fine
  categories: PlayerLocation
  condition: (0 0) to (99 99)
  script:
  - message
  - I want a round room
  - at the end of the day
  - end message
  - remove trigger
";
        }

        public static CompoundMapBridge Get_RR_Bridge()
        {
            var bridge = new CompoundMapBridge
            {
                Name = "Round Room",
                Terrain = new List<string>
                {
                    "..###..",
                    ".##.##.",
                    ".#...+.",
                    ".##.##.",
                    "..###..",
                },
                Width = 7,
                Height = 5,
            };

            var trig = new Trigger
            {
                Categories = TriggerCategories.PlayerLocation,
                Condition = "(0 0) to (99 99)",
            };
            trig.Script.Add("message");
            trig.Script.Add("I want a round room");
            trig.Script.Add("at the end of the day");
            trig.Script.Add("end message");
            trig.Script.Add("remove trigger");
            bridge.Triggers.Add(trig);

            bridge.Legend["."] = "soil";
            bridge.Legend["#"] = "stone wall";
            bridge.Legend["+"] = "closed door";

            return bridge;
        }


    }
}
