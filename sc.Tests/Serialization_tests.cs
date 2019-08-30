using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using SadConsole;
using Newtonsoft.Json;
using NUnit.Framework;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System;
using CopperBend.Persist;
using Newtonsoft.Json.Converters;

namespace sc_tests
{
    //  Throwing stuff at the wall  --  This is all mad dog prototyping
    //  CRT_* tests that we "can round-trip" an object, serialize then deserialize


    [TestFixture]
    public class Dser_ItemMap_Tests
    {
        [SetUp]
        public void SetUp()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ContractResolver = DomainContractResolver.Instance,
            };
        }


        [Test]
        public void CRT_Item()
        {
            var item = new Item((0, 0))
            {
                Name = "Fluffy",
                Glyph = '*',
                Adjective = "So",
                Quantity = 120,
                Foreground = Color.BlanchedAlmond,
                IsUsable = false,
            };

            var json = JsonConvert.SerializeObject(item);
            var newItem = JsonConvert.DeserializeObject<Item>(json);
            Assert.That(newItem.Name, Is.EqualTo(item.Name));
            Assert.That(newItem.Adjective, Is.EqualTo(item.Adjective));
            Assert.That(newItem.Glyph, Is.EqualTo(item.Glyph));
            Assert.That(newItem.Quantity, Is.EqualTo(item.Quantity));
            Assert.That(newItem.Foreground, Is.EqualTo(item.Foreground));
            Assert.That(newItem.IsUsable, Is.EqualTo(item.IsUsable));
        }

        [Test]
        public void CRT_IItem()
        {
            IItem item = new Item((0, 0))
            {
                Name = "Fluffy",
                Glyph = '*',
                Adjective = "So",
                Quantity = 120,
                Foreground = Color.BlanchedAlmond,
                IsUsable = false,
            };

            var json = JsonConvert.SerializeObject(item);
            IItem newItem = JsonConvert.DeserializeObject<IItem>(json);
            Assert.That(newItem.Name, Is.EqualTo(item.Name));
            Assert.That(newItem.Adjective, Is.EqualTo(item.Adjective));
            Assert.That(newItem.Glyph, Is.EqualTo(item.Glyph));
            Assert.That(newItem.Quantity, Is.EqualTo(item.Quantity));
            Assert.That(newItem.Foreground, Is.EqualTo(item.Foreground));
            Assert.That(newItem.IsUsable, Is.EqualTo(item.IsUsable));

            Assert.That(newItem.ItemType, Is.EqualTo("Item"));
        }

        [Test]
        public void CRT_IItem_Knife()
        {
            IItem item = new Knife((2,4))
            {
                Adjective = "TSA-Approved",
                Quantity = 1,
                Foreground = Color.BlanchedAlmond,
                IsUsable = true,
            };

            var json = JsonConvert.SerializeObject(item);
            IItem newItem = JsonConvert.DeserializeObject<IItem>(json);
            Assert.That(newItem.Name, Is.EqualTo(item.Name));
            Assert.That(newItem.Adjective, Is.EqualTo(item.Adjective));
            Assert.That(newItem.Glyph, Is.EqualTo(item.Glyph));
            Assert.That(newItem.Quantity, Is.EqualTo(item.Quantity));
            Assert.That(newItem.Foreground, Is.EqualTo(item.Foreground));
            Assert.That(newItem.IsUsable, Is.EqualTo(item.IsUsable));

            Assert.That(newItem.ItemType, Is.EqualTo("Knife"));
            Assert.That(newItem.Location, Is.EqualTo((2,4)));
        }

        [Test]
        public void CRT_Empty_ItemMap()
        {
            var map = new ItemMap { MyName = "Bobby" };
            map.Add(new Knife((3, 3)), (3, 3));
            var json = JsonConvert.SerializeObject(map);

            //Debugger.Launch();
            var newMap = JsonConvert.DeserializeObject<ItemMap>(json);
            Assert.That(newMap, Is.Not.Null);
            Assert.That(newMap.Count, Is.EqualTo(1));
            //Assert.That(newMap.MyName, Is.EqualTo("Bobby"));
            Knife knife = newMap.Items.ElementAt(0) as Knife;
            Assert.That(knife.ItemType, Is.EqualTo("Knife"));
            Assert.That(knife.Location, Is.EqualTo((3,3)));
        }
    }


    public class DomainContractResolver : DefaultContractResolver
    {
        public static readonly DomainContractResolver Instance = new DomainContractResolver();

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (objectType == typeof(ItemMap))
            {
                contract.Converter = new ItemMapConverter();
            }
            if (typeof(IItem).IsAssignableFrom(objectType))
            {
                contract.Converter = new Converter_of_IItem();
            }

            return contract;
        }
    }

    [TestFixture]
    public class JsonSerialTests
    {
        [SetUp]
        public void SetUp()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ContractResolver = DomainContractResolver.Instance
            };
        }



        //[Test]
        //public void CRT_PlantDetails()
        //{
        //    var details = new PlantDetails
        //    {
        //        FruitAdjective = "Zesty",
        //        FruitKnown = true,
        //        GrowthTime = 88,
        //        MainName = "Lemon",
        //        SeedAdjective = "Pale",
        //        SeedKnown = true,
        //    };

        //    var json = JsonConvert.SerializeObject(details);
        //    var newDetails = JsonConvert.DeserializeObject<PlantDetails>(json);
        //    Assert.That(newDetails.FruitAdjective, Is.EqualTo(details.FruitAdjective));
        //    Assert.That(newDetails.FruitKnown, Is.EqualTo(details.FruitKnown));
        //    Assert.That(newDetails.GrowthTime, Is.EqualTo(details.GrowthTime));
        //    Assert.That(newDetails.MainName, Is.EqualTo(details.MainName));
        //    Assert.That(newDetails.SeedAdjective, Is.EqualTo(details.SeedAdjective));
        //    Assert.That(newDetails.SeedKnown, Is.EqualTo(details.SeedKnown));
        //}

        //[Test]
        //public void CRT_Fruit()
        //{
        //    var details = new PlantDetails
        //    {
        //        FruitAdjective = "Zesty",
        //    };
        //    var fruit = new Fruit((0, 0), 1, details)
        //    {
        //        Name = "Fluffy",
        //        Glyph = '*',
        //        Adjective = "So",
        //        Quantity = 120,
        //        Foreground = Color.BlanchedAlmond,
        //        IsUsable = false,
        //    };

        //    var json = JsonConvert.SerializeObject(fruit);
        //    var newFruit = JsonConvert.DeserializeObject<Fruit>(json);
        //    Assert.That(newFruit.Name, Is.EqualTo(fruit.Name));
        //    Assert.That(newFruit.Adjective, Is.EqualTo(fruit.Adjective));
        //    Assert.That(newFruit.Glyph, Is.EqualTo(fruit.Glyph));
        //    Assert.That(newFruit.Quantity, Is.EqualTo(fruit.Quantity));
        //    Assert.That(newFruit.Foreground, Is.EqualTo(fruit.Foreground));
        //    Assert.That(newFruit.IsUsable, Is.EqualTo(fruit.IsUsable));
        //}

        //[Test]
        //public void CRT_Cell()
        //{
        //    var cell = new Cell
        //    {
        //        Background = Color.AliceBlue,
        //        Foreground = Color.AliceBlue,
        //        Glyph = '@',
        //        IsVisible = true,
        //    };

        //    var json = JsonConvert.SerializeObject(cell);
        //    //System.Console.Error.WriteLine(json);
        //    Cell newCell = JsonConvert.DeserializeObject<Cell>(json);

        //    Assert.That(newCell, Is.Not.Null);
        //    Assert.That(newCell.Background, Is.EqualTo(Color.AliceBlue));
        //    Assert.That(newCell.Foreground, Is.EqualTo(Color.AliceBlue));
        //    Assert.That(newCell.Glyph, Is.EqualTo('@'));
        //}

        //[Test]
        //public void CRT_TerrainType()
        //{
        //    var tt = new TerrainType
        //    {
        //        Name = "dirt road",
        //        CanPlant = false,
        //        CanSeeThrough = true,
        //        CanWalkThrough = true,
        //    };
        //    var json = JsonConvert.SerializeObject(tt);
        //    //System.Console.Error.WriteLine(json);
        //    var newTT = JsonConvert.DeserializeObject<TerrainType>(json);

        //    Assert.That(newTT.Name, Is.EqualTo(tt.Name));
        //    Assert.That(newTT.CanPlant, Is.EqualTo(tt.CanPlant));
        //    Assert.That(newTT.CanSeeThrough, Is.EqualTo(tt.CanSeeThrough));
        //    Assert.That(newTT.CanWalkThrough, Is.EqualTo(tt.CanWalkThrough));
        //}


        //[Test]
        //public void CRT_Space()
        //{
        //    var space = new Space
        //    {
        //        IsKnown = true,
        //        IsSown = true,
        //        IsTilled = true
        //    };

        //    var json = JsonConvert.SerializeObject(space);
        //    //System.Console.Error.WriteLine(json);
        //    var newSpace = JsonConvert.DeserializeObject<Space>(json);

        //    Assert.That(newSpace.IsKnown);
        //    Assert.That(newSpace.IsSown);
        //    Assert.That(newSpace.IsTilled);
        //}

        //[Test]
        //public void CRT_SpaceMap()
        //{
        //    var map = new SpaceMap(5, 5)
        //    {
        //        PlayerStartPoint = (3, 3)
        //    };

        //    map.AddItem(new Space(888), (4, 4));

        //    var json = JsonConvert.SerializeObject(map);
        //    Debugger.Launch();
        //    //System.Console.Error.WriteLine(json);
        //    var newMap = JsonConvert.DeserializeObject<SpaceMap>(json);

        //    Assert.That(newMap.Height, Is.EqualTo(5));
        //    Assert.That(newMap.Width, Is.EqualTo(5));
        //    Assert.That(newMap.PlayerStartPoint, Is.EqualTo((3, 3)));

        //    var space = newMap.GetItem((4, 4));
        //    Assert.That(space, Is.Not.Null);
        //}

        //[Test]
        //public void CRT_BlightMap()
        //{
        //    //0.2: int ctor arg = deserializing workaround
        //    var map = new BlightMap(1) { Name = "Bofungus" };
        //    var blight = new AreaBlight(888) { Extent = 11 };
        //    map.AddItem(blight, (7, 11));
        //    map.AddItem(new AreaBlight() { Extent = 8 }, (7, 12));

        //    var json = JsonConvert.SerializeObject(map);
        //    //System.Console.Error.WriteLine(json);
        //    //Debugger.Launch();
        //    var newMap = JsonConvert.DeserializeObject<BlightMap>(json);

        //    Assert.That(newMap.Name, Is.EqualTo("Bofungus"));
        //    var entry = newMap.GetItem((7, 11));
        //    Assert.That(entry, Is.Not.Null);
        //    Assert.That(entry.ID, Is.EqualTo(888));
        //}


        //[Test]
        //public void CRT_SpaceMap_down_to_Cell()
        //{
        //    Cell originalCell = new Cell
        //    {
        //        Glyph = '~',
        //    };

        //    var originalTerrain = new TerrainType
        //    {
        //        Looks = originalCell
        //    };

        //    var originalSpace = new Space(888)
        //    {
        //        Terrain = originalTerrain
        //    };

        //    var map = new SpaceMap(6, 6);

        //    map.AddItem(originalSpace, (4, 4));

        //    var json = JsonConvert.SerializeObject(map);
        //    //System.Console.Error.WriteLine(json);
        //    var newMap = JsonConvert.DeserializeObject<SpaceMap>(json);

        //    var space = newMap.GetItem((4, 4));
        //    var cell = space.Terrain.Looks;
        //    Assert.That(cell, Is.Not.Null);
        //    Assert.That(cell.Glyph, Is.EqualTo('~'));
        //}

        //[Test]
        //public void CRT_AreaBlight()
        //{
        //    var blight = new AreaBlight() { Extent = 14 };

        //    var json = JsonConvert.SerializeObject(blight);
        //    //System.Console.Error.WriteLine(json);
        //    var newBlight = JsonConvert.DeserializeObject<AreaBlight>(json);

        //    Assert.That(newBlight.Extent, Is.EqualTo(14));
        //}

        //public static byte[] ObjectToByteArray(object obj)
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    using (var ms = new MemoryStream())
        //    {
        //        bf.Serialize(ms, obj);
        //        return ms.ToArray();
        //    }
        //}

        //public static object ByteArrayToObject(byte[] arrBytes)
        //{
        //    using (var memStream = new MemoryStream(arrBytes))
        //    {
        //        var binForm = new BinaryFormatter();
        //        var obj = binForm.Deserialize(memStream);
        //        return obj;
        //    }
        //}
    }
}
