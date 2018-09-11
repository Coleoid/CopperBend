using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
            Assert.That(map.Terrain[0,0], Is.EqualTo(TerrainType.Stone));
        }

        [Test]
        public void Work_YAML_with_low_level_stream()
        {
           var reader = new StringReader(RoundRoomYaml);

            var yaml = new YamlStream();
            yaml.Load(reader);

            var mapping =
                (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var entry in mapping.Children)
            {
                var entryName = ((YamlScalarNode) entry.Key).Value;

                var entryValue = ((YamlScalarNode) entry.Value).Value;
                Console.WriteLine($"{entryName}: {entryValue}");
            }
        }

        [Test]
        public void YAML_to_DTO()
        {
            var reader = new StringReader(RoundRoomYaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var dto = deserializer.Deserialize<MapDTO>(reader);
            Assert.That(dto.Name, Is.EqualTo("The Round Room"));
            Assert.That(dto.Legend.Count(), Is.EqualTo(3));
            Assert.That(dto.Legend.ContainsKey("+"));
            Assert.That(dto.Legend.ContainsKey("#"));
            Assert.That(dto.Legend["."], Is.EqualTo("Dirt"));
            Assert.That(dto.Terrain.Count(), Is.EqualTo(6));
            Assert.That(dto.Terrain[1], Is.EqualTo("..#..#.."));

            //  Which is great--Now how do I put this into the R# map?
        }

        public void DTO_to_map()
        {
            var dto = new MapDTO
            {
                Name = "The Round Room",
                Terrain = new List<string> { "#" }
            };


        }

        public class MapDTO
        {
            public string Name { get; set; }
            public Dictionary<string,string> Legend { get; set; }
            public List<string> Terrain { get; set; }
        }

        private const string RoundRoomYaml = @"---
name:  The Round Room

legend:
 '.': Dirt
 '#': StoneWall
 '+': Door

terrain:
 - ...##...
 - ..#..+..
 - .#....#.
 - .#....#.
 - ..#..#..
 - ...##...
";
    }
}
