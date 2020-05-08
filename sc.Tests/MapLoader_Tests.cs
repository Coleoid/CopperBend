using System.Collections.Generic;
using log4net;
using CopperBend.Fabric;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.Persist.Tests
{
    [TestFixture]
    public class MapLoaderTests
    {
        private ILog __log;
        private MapLoader _loader;

        [SetUp]
        public void SetUp()
        {
            __log = Substitute.For<ILog>();
            var publisher = new BookPublisher(null);
            Atlas atlas = publisher.Atlas_FromNew();
            _loader = new MapLoader(__log, atlas);
            Space.SetIDGenerator(new GoRogue.IDGenerator());
        }

        [Test]
        public void DataFromYAML()
        {
            var data = _loader.DataFromYAML(RoundRoomYaml);
            Assert_CMD_isRoundRoom(data);
        }

        [Test]
        public void Data_YAML_Data()
        {
            var rrData = GetRoundRoomData();
            var yaml = _loader.YAMLFromData(rrData);
            var data = _loader.DataFromYAML(yaml);
            Assert_CMDs_equiv(data, rrData);
        }

        [Test]
        public void Data_Map_Data()
        {
            var rrData = GetRoundRoomData();
            var map = _loader.MapFromData(rrData);
            var data = _loader.DataFromMap(map);
            Assert_CMDs_equiv(data, rrData);
        }

        [Test]
        public void MapFromYAML()
        {
            var map = _loader.MapFromYAML(RoundRoomYaml);

            Assert_Map_isRoundRoom(map);
        }

        [Test]
        public void MapFromData()
        {
            var data = GetRoundRoomData();

            var map = _loader.MapFromData(data);
            Assert_Map_isRoundRoom(map);
        }

        private void Assert_CMD_isRoundRoom(CompoundMapData data)
        {
            Assert_CMDs_equiv(data, GetRoundRoomData());
        }

        private void Assert_CMDs_equiv(CompoundMapData result, CompoundMapData expected)
        {
            Assert.That(result.Name, Is.EqualTo(expected.Name));
            // Checking values, not keys, since the legend keys may change
            foreach (var val in expected.Legend.Values)
            {
                Assert.That(result.Legend.ContainsValue(val), $"Value [{val}] not found in resulting CMD legend.");
                //Assert.That(result.Legend[key], Is.EqualTo(expected.Legend[key]));
            }
            Assert.That(result.Legend.Count, Is.EqualTo(expected.Legend.Count));

            for (int row = 0; row < expected.Terrain.Count; row++)
            {
                Assert.That(result.Terrain[row], Is.EqualTo(expected.Terrain[row]), $"Row {row} didn't match");
            }
            Assert.That(result.Terrain.Count, Is.EqualTo(expected.Terrain.Count));
        }

        private void Assert_Map_isRoundRoom(CompoundMap map)
        {
            Assert.That(map.Name, Is.EqualTo("Round Room"));
            Assert.That(map.Height, Is.EqualTo(5));
            Assert.That(map.Width, Is.EqualTo(7));
            Assert.That(map.SpaceMap.GetItem((1, 0)).Terrain.Name, Is.EqualTo("soil"));
            Assert.That(map.SpaceMap.GetItem((1, 1)).Terrain.Name, Is.EqualTo("stone wall"));
            Assert.That(map.SpaceMap.GetItem((5, 2)).Terrain.Name, Is.EqualTo("closed door"));
        }

        private CompoundMapData GetRoundRoomData()
        {
            var data = new CompoundMapData
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
            };

            data.FirstSightMessage.Add("I want a round room at the end of the day");

            data.Legend["."] = "soil";
            data.Legend["#"] = "stone wall";
            data.Legend["+"] = "closed door";

            return data;
        }

        private const string RoundRoomYaml = @"---
name:  Round Room
firstSightMessage:
 - I want a round room at the end of the day

legend:
 '.': soil
 '#': stone wall
 '+': closed door

terrain:
 - ..###..
 - .##.##.
 - .#...+.
 - .##.##.
 - ..###..
";
    }
}
