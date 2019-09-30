//using SadConsole;
//using Newtonsoft.Json;
//using CopperBend.Contract;
//using CopperBend.Fabric;
//using CopperBend.Model;
//using NUnit.Framework;
//using CopperBend.Engine;

//namespace sc_tests
//{
//    [TestFixture]
//    public class Serial_Map_Tests
//    {
//        [SetUp]
//        public void SetUp()
//        {
//            Engine.Cosmogenesis("seed");
//        }

//        [Test]
//        public void CRT_TerrainType()
//        {
//            var tt = new TerrainType
//            {
//                Name = "dirt road",
//                CanPlant = false,
//                CanSeeThrough = true,
//                CanWalkThrough = true,
//            };
//            var json = JsonConvert.SerializeObject(tt);
//            //System.Console.Error.WriteLine(json);
//            var newTT = JsonConvert.DeserializeObject<TerrainType>(json);

//            Assert.That(newTT.Name, Is.EqualTo(tt.Name));
//            Assert.That(newTT.CanPlant, Is.EqualTo(tt.CanPlant));
//            Assert.That(newTT.CanSeeThrough, Is.EqualTo(tt.CanSeeThrough));
//            Assert.That(newTT.CanWalkThrough, Is.EqualTo(tt.CanWalkThrough));
//        }

//        [Test]
//        public void CRT_Space()
//        {
//            var space = new Space
//            {
//                IsKnown = true,
//                IsSown = true,
//                IsTilled = true
//            };

//            var json = JsonConvert.SerializeObject(space);

//            //if (!Debugger.IsAttached) Debugger.Launch();
//            var newSpace = JsonConvert.DeserializeObject<Space>(json);

//            Assert.That(newSpace.IsKnown);
//            Assert.That(newSpace.IsSown);
//            Assert.That(newSpace.IsTilled);
//        }

//        [Test]
//        public void CRT_SpaceMap()
//        {
//            var map = new SpaceMap(5, 5)
//            {
//                PlayerStartPoint = (3, 3)
//            };

//            map.AddItem(new Space(888), (4, 4));

//            var json = JsonConvert.SerializeObject(map);
//            //Debugger.Launch();
//            //System.Console.Error.WriteLine(json);
//            var newMap = JsonConvert.DeserializeObject<SpaceMap>(json);

//            Assert.That(newMap.Height, Is.EqualTo(5));
//            Assert.That(newMap.Width, Is.EqualTo(5));
//            Assert.That(newMap.PlayerStartPoint, Is.EqualTo((3, 3)));

//            var space = newMap.GetItem((4, 4));
//            Assert.That(space, Is.Not.Null);
//        }

//        [Test]
//        public void CRT_AreaBlight()
//        {
//            var blight = new AreaBlight(22) { Health = 14 };

//            var json = JsonConvert.SerializeObject(blight);
//            //System.Console.Error.WriteLine(json);
//            var newBlight = JsonConvert.DeserializeObject<IAreaBlight>(json);

//            Assert.That(newBlight.ID, Is.EqualTo(22));
//            Assert.That(newBlight.Health, Is.EqualTo(14));
//        }

//        [Test]
//        public void CRT_BlightMap()
//        {
//            //0.2: int ctor arg = deserializing workaround
//            var map = new BlightMap(1) { Name = "Bofungus" };
//            var blight = new AreaBlight(888) { Health = 11 };
//            map.AddItem(blight, (7, 11));
//            map.AddItem(new AreaBlight() { Health = 8 }, (7, 12));

//            var json = JsonConvert.SerializeObject(map);
//            //System.Console.Error.WriteLine(json);
//            //Debugger.Launch();
//            var newMap = JsonConvert.DeserializeObject<BlightMap>(json);

//            Assert.That(newMap.Name, Is.EqualTo("Bofungus"));
//            var entry = newMap.GetItem((7, 11));
//            Assert.That(entry, Is.Not.Null);
//            Assert.That(entry.ID, Is.EqualTo(888));
//        }

//        [Test]
//        public void CRT_SpaceMap_down_to_Cell()
//        {
//            Cell originalCell = new Cell
//            {
//                Glyph = '~',
//            };

//            var originalTerrain = new TerrainType
//            {
//                Looks = originalCell
//            };

//            var originalSpace = new Space(888)
//            {
//                Terrain = originalTerrain
//            };

//            var map = new SpaceMap(6, 6);

//            map.AddItem(originalSpace, (4, 4));

//            var json = JsonConvert.SerializeObject(map);
//            //System.Console.Error.WriteLine(json);
//            var newMap = JsonConvert.DeserializeObject<SpaceMap>(json);

//            var space = newMap.GetItem((4, 4));
//            var cell = space.Terrain.Looks;
//            Assert.That(cell, Is.Not.Null);
//            Assert.That(cell.Glyph, Is.EqualTo('~'));
//        }

//        //TODO: Bring these back to use when serializing random
//        //public static byte[] ObjectToByteArray(object obj)
//        //{
//        //    BinaryFormatter bf = new BinaryFormatter();
//        //    using (var ms = new MemoryStream())
//        //    {
//        //        bf.Serialize(ms, obj);
//        //        return ms.ToArray();
//        //    }
//        //}

//        //public static object ByteArrayToObject(byte[] arrBytes)
//        //{
//        //    using (var memStream = new MemoryStream(arrBytes))
//        //    {
//        //        var binForm = new BinaryFormatter();
//        //        var obj = binForm.Deserialize(memStream);
//        //        return obj;
//        //    }
//        //}
//    }
//}
