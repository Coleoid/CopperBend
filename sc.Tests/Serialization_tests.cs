using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using SadConsole;
using Newtonsoft.Json;
using NUnit.Framework;
using CopperBend.Contract;
using CopperBend.Fabric;

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
            var map = new SpaceMap(5, 5)
            {
                PlayerStartPoint = (3, 3)
            };

            map.AddItem(new Space(888), (4, 4));

            var json = JsonConvert.SerializeObject(map);
            //Debugger.Launch();
            //System.Console.Error.WriteLine(json);
            var newMap = JsonConvert.DeserializeObject<SpaceMap>(json);

            Assert.That(newMap.Height, Is.EqualTo(5));
            Assert.That(newMap.Width, Is.EqualTo(5));
            Assert.That(newMap.PlayerStartPoint, Is.EqualTo((3, 3)));

            var space = newMap.GetItem((4, 4));
            Assert.That(space, Is.Not.Null);
        }

        [Test]
        public void Can_roundtrip_BlightMap()
        {
            //0.2: int ctor arg = deserializing workaround
            var map = new BlightMap(1) { Name = "Bofungus" };
            var blight = new AreaBlight(888) { Extent = 11 };
            map.AddItem(blight, (7, 11));
            map.AddItem(new AreaBlight() { Extent = 8 }, (7, 12));

            var json = JsonConvert.SerializeObject(map);
            //System.Console.Error.WriteLine(json);
            //Debugger.Launch();
            var newMap = JsonConvert.DeserializeObject<BlightMap>(json);

            Assert.That(newMap.Name, Is.EqualTo("Bofungus"));
            var entry = newMap.GetItem((7, 11));
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.ID, Is.EqualTo(888));
        }


        [Test]
        public void Can_roundtrip_SpaceMap_down_to_Cell()
        {
            Cell originalCell = new Cell
            {
                Glyph = '~',
            };

            var originalTerrain = new TerrainType
            {
                Looks = originalCell
            };

            var originalSpace = new Space(888)
            {
                Terrain = originalTerrain
            };

            var map = new SpaceMap(6, 6);

            map.AddItem(originalSpace, (4, 4));

            var json = JsonConvert.SerializeObject(map);
            //System.Console.Error.WriteLine(json);
            var newMap = JsonConvert.DeserializeObject<SpaceMap>(json);

            var space = newMap.GetItem((4, 4));
            var cell = space.Terrain.Looks;
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.Glyph, Is.EqualTo('~'));
        }

        [Test]
        public void Can_roundtrip_AreaBlight()
        {
            var blight = new AreaBlight() { Extent = 14 };

            var json = JsonConvert.SerializeObject(blight);
            //System.Console.Error.WriteLine(json);
            var newBlight = JsonConvert.DeserializeObject<AreaBlight>(json);

            Assert.That(newBlight.Extent, Is.EqualTo(14));
        }

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
