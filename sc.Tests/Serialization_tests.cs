using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using SadConsole;
using Newtonsoft.Json;
using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.RepresentationModel;
using CopperBend.Contract;
using CopperBend.Engine;
using CopperBend.Fabric;
using GoRogue;

namespace sc_tests
{
    //  Throwing stuff at the wall  --  This is all mad dog prototyping

    [TestFixture]
    public class JsonSerialTests
    {
        [SetUp]
        public void SetUp()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
        }

        [Test]
        public void Can_roundtrip_Cell()
        {
            var cell = new Cell
            {
                Background = Color.AliceBlue,
                Foreground = Color.AliceBlue,
                Glyph = '@',
                IsVisible = true,
            };

            var json = JsonConvert.SerializeObject(cell);
            //System.Console.Error.WriteLine(json);
            Cell newCell = JsonConvert.DeserializeObject<Cell>(json);

            Assert.That(newCell, Is.Not.Null);
            Assert.That(newCell.Background, Is.EqualTo(Color.AliceBlue));
            Assert.That(newCell.Foreground, Is.EqualTo(Color.AliceBlue));
            Assert.That(newCell.Glyph, Is.EqualTo('@'));
        }

        [Test]
        public void Can_roundtrip_TerrainType()
        {
            var tt = new TerrainType
            {
                Name = "dirt road",
                CanPlant = false,
                CanSeeThrough = true,
                CanWalkThrough = true,
            };
            var json = JsonConvert.SerializeObject(tt);
            //System.Console.Error.WriteLine(json);
            var newTT = JsonConvert.DeserializeObject<TerrainType>(json);

            Assert.That(newTT.Name, Is.EqualTo(tt.Name));
            Assert.That(newTT.CanPlant, Is.EqualTo(tt.CanPlant));
            Assert.That(newTT.CanSeeThrough, Is.EqualTo(tt.CanSeeThrough));
            Assert.That(newTT.CanWalkThrough, Is.EqualTo(tt.CanWalkThrough));
        }


        [Test]
        public void Can_roundtrip_Space()
        {
            var space = new Space
            {
                IsKnown = true,
                IsSown = true,
                IsTilled = true
            };

            var json = JsonConvert.SerializeObject(space);
            //System.Console.Error.WriteLine(json);
            var newSpace = JsonConvert.DeserializeObject<Space>(json);

            Assert.That(newSpace.IsKnown);
            Assert.That(newSpace.IsSown);
            Assert.That(newSpace.IsTilled);
        }

        [Test]
        public void Can_roundtrip_SpaceMap()
        {
            var map = new SpaceMap(2, 2)
            {
                PlayerStartPoint = (3, 3)
            };

            map.AddSpace(new Space(888), (4, 4));

            var json = JsonConvert.SerializeObject(map);
            //System.Console.Error.WriteLine(json);
            var newMap = JsonConvert.DeserializeObject<SpaceMap>(json);

            Assert.That(newMap.Height, Is.EqualTo(2));
            Assert.That(newMap.Width, Is.EqualTo(2));
            Assert.That(newMap.PlayerStartPoint, Is.EqualTo((3, 3)));

            var space = newMap.GetSpace((4, 4));
            Assert.That(space, Is.Not.Null);
        }


        [Test]
        public void Can_roundtrip_SpaceMap_Spaces()
        {
            var map = new SpaceMap(2, 2)
            {
            };

            map.AddSpace(new Space(888), (4, 4));

            var json = JsonConvert.SerializeObject(map);
            System.Console.Error.WriteLine(json);
            var newMap = JsonConvert.DeserializeObject<SpaceMap>(json);

            var space = newMap.GetSpace((4, 4));
            Assert.That(space, Is.Not.Null);
            Assert.That(space.ID, Is.EqualTo(888));
        }
    }



    public class SpaceMapSaveData
    {
        public SpaceMapSaveData(SpaceMap map)
        {
            Width = map.Width;
            Height = map.Height;
        }

        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class CompoundMapSaveData
    {
        public CompoundMapSaveData(CompoundMap map)
        {
            SpaceMap = new SpaceMapSaveData(map.SpaceMap);
        }

        public SpaceMapSaveData SpaceMap { get; set; }
    }

    [TestFixture]
    public class Serialization_tests
    {
        //[Test]
        public void yaml_SpaceMap()
        {
            var smap = new SpaceMap(2, 2);
            var cmap = new CompoundMap();
            cmap.SpaceMap = smap;
            smap.AddSpace(new Space(), (0, 0));
            smap.AddSpace(new Space(), (0, 1));
            smap.AddSpace(new Space(), (1, 0));
            smap.AddSpace(new Space(), (1, 1));

            var saveData = new CompoundMapSaveData(cmap);

            //var serializer = new SerializerBuilder().Build();
            //var yaml = serializer.Serialize(saveData);
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(cmap);

            //var serializer = new Serializer();
            //string output = null;
            //using (StringWriter writer = new StringWriter())
            //{
            //    serializer.Serialize(writer, saveData);
            //    output = writer.ToString();
            //}

            //System.Console.Error.WriteLine(yaml);
            //System.Diagnostics.Debugger.Launch();
        }

        [Test]
        public void yaml_MapData()
        {
            var md = new MapData();
            md.Name = "map name";
            BlightOverlayData overlay = new BlightOverlayData { Location = "here", Name = "Frank" };
            overlay.Terrain.Add("ava987f076a");
            md.Blight.Add(overlay);
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(md);
            //System.Console.Error.WriteLine(yaml);
        }

        public void Read___()
        {
            using (StreamReader reader = File.OpenText("path-and-name-of-your-yaml-file"))
            {
                var stream = new YamlStream();
                stream.Load(reader);
            }

        }

        public void Write___()
        {
            using (TextWriter writer = new StringWriter())
            {
                var stream = new YamlStream();
                stream.Save(writer);
            }

        }

        //[Test]
        //public void Serdeser_GameState()
        //{
        //    var gameState = new GameState();

        //    var bytes = ObjectToByteArray(gameState);
        //    var newState = (GameState)ByteArrayToObject(bytes);

        //    Assert.That(gameState.Map, Is.EqualTo(newState.Map));
        //}

        public static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream(arrBytes))
            {
                var binForm = new BinaryFormatter();
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

    }
}
