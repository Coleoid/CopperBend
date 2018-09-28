using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class MapLoaderTests
    {
        [Test]
        public void LoadMap_returns_something()
        {
            var loader = new MapLoader();

            var map = loader.LoadMap("test:block");

            Assert.That(map, Is.Not.Null);
            Assert.That(map.Tiles[0,0], Is.EqualTo(TerrainType.StoneWall));
        }

        [Test]
        public void YAML_to_DTO()
        {
            var loader = new MapLoader();
            var dto = loader.DataFromYAML(RoundRoomYaml);

            Assert.That(dto.Name, Is.EqualTo("The Round Room"));
            Assert.That(dto.Legend.Count(), Is.EqualTo(3));
            Assert.That(dto.Legend.ContainsKey("+"));
            Assert.That(dto.Legend.ContainsKey("#"));
            Assert.That(dto.Legend["."], Is.EqualTo("Dirt"));
            Assert.That(dto.Terrain.Count(), Is.EqualTo(5));
            Assert.That(dto.Terrain[1], Is.EqualTo(".##..##."));
        }

        [Test]
        public void YAML_to_Map()
        {
            var loader = new MapLoader();
            var map = loader.MapFromYAML(RoundRoomYaml);

            Assert.That(map.Name, Is.EqualTo("The Round Room"));
            Assert.That(map.Height, Is.EqualTo(5));
            Assert.That(map.Width, Is.EqualTo(8));
            Assert.That(map.Tiles[1, 0], Is.EqualTo(TerrainType.Dirt));
            Assert.That(map.Tiles[1, 1], Is.EqualTo(TerrainType.StoneWall));
       }

        [Test]
        public void DTO_to_map()
        {
            var dto = new MapData
            {
                Name = "The Round Room",
                Terrain = new List<string> { "#" }
            };
        }

        private const string RoundRoomYaml = @"---
name:  The Round Room

legend:
 '.': Dirt
 '#': StoneWall
 '+': ClosedDoor

terrain:
 - ..####..
 - .##..##.
 - .#....+.
 - .##..##.
 - ...##...
";
    }
}
