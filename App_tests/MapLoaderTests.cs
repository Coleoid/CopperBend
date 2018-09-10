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

            // Load the stream
            var yaml = new YamlStream();
            yaml.Load(reader);

            // Examine the stream
            var mapping =
                (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var entry in mapping.Children)
            {
                var entryName = ((YamlScalarNode) entry.Key).Value;


                var entryValue = ((YamlScalarNode) entry.Value).Value;
                Console.WriteLine($"{entryName}: {entryValue}");
            }

            //// List all the items
            //var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("items")];
            //foreach (YamlMappingNode item in items)
            //{
            //    Console.WriteLine(
            //        "{0}\t{1}",
            //        item.Children[new YamlScalarNode("part_no")],
            //        item.Children[new YamlScalarNode("descrip")]
            //    );
            //}
        }

        [Test]
        public void Deserialize_to_POCO()
        {
            var reader = new StringReader(RoundRoomYaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var map = deserializer.Deserialize<MapData>(reader);
            Assert.That(map.Name, Is.EqualTo("The Round Room"));
            Assert.That(map.Legend.Count(), Is.EqualTo(3));
            Assert.That(map.Legend.ContainsKey("+"));
            Assert.That(map.Legend.ContainsKey("#"));
            Assert.That(map.Legend["."], Is.EqualTo("Dirt"));
            Assert.That(map.Terrain.Count(), Is.EqualTo(6));
            Assert.That(map.Terrain[1], Is.EqualTo("..#..#.."));

            //  Which is beautiful--Now how do I put this into the R# map?
        }

        public class MapData
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
