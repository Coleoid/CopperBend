using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

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
        public void LoadSomeYAML()
        {
            // Setup the input
            var input = new StringReader(Document);

            // Load the stream
            var yaml = new YamlStream();
            yaml.Load(input);

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

        private const string Document = @"---
name:  The Round Room

legend:
 '.': Dirt
 '#': StoneWall

terrain:
 - ...##...
 - ..#..#..
 - .#....#.
 - .#....#.
 - ..#..#..
 - ...##...
";
    }
}
