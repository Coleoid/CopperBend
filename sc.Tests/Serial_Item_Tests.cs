using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using CopperBend.Persist;
using NUnit.Framework;

namespace sc_tests
{
    //  CRT_* tests that we "can round-trip" an object, serialize then deserialize
    [TestFixture]
    public class Serial_Item_Tests
    {
        [SetUp]
        public void SetUp()
        {
            Item.IDGenerator = new GoRogue.IDGenerator();
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
            IItem item = new Knife((2, 4))
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
            Assert.That(newItem.Location, Is.EqualTo((2, 4)));
        }

        [Test]
        public void CRT_Empty_ItemMap()
        {
            var map = new ItemMap {MyName = "Bobby"};
            map.Add(new Knife((3, 3)), (3, 3));
            var json = JsonConvert.SerializeObject(map);

            //Debugger.Launch();
            var newMap = JsonConvert.DeserializeObject<ItemMap>(json);
            Assert.That(newMap, Is.Not.Null);
            Assert.That(newMap.Count, Is.EqualTo(1));
            //Assert.That(newMap.MyName, Is.EqualTo("Bobby"));
            Knife knife = newMap.Items.ElementAt(0) as Knife;
            Assert.That(knife.ItemType, Is.EqualTo("Knife"));
            Assert.That(knife.Location, Is.EqualTo((3, 3)));
        }


        [Test]
        public void CRT_PlantDetails()
        {
            var details = new PlantDetails
            {
                FruitAdjective = "Zesty",
                FruitKnown = true,
                GrowthTime = 88,
                MainName = "Lemon",
                SeedAdjective = "Pale",
                SeedKnown = true,
            };

            var json = JsonConvert.SerializeObject(details);
            var newDetails = JsonConvert.DeserializeObject<PlantDetails>(json);
            Assert.That(newDetails.FruitAdjective, Is.EqualTo(details.FruitAdjective));
            Assert.That(newDetails.FruitKnown, Is.EqualTo(details.FruitKnown));
            Assert.That(newDetails.GrowthTime, Is.EqualTo(details.GrowthTime));
            Assert.That(newDetails.MainName, Is.EqualTo(details.MainName));
            Assert.That(newDetails.SeedAdjective, Is.EqualTo(details.SeedAdjective));
            Assert.That(newDetails.SeedKnown, Is.EqualTo(details.SeedKnown));
        }

        [Test]
        public void CRT_Fruit()
        {
            var details = new PlantDetails
            {
                FruitAdjective = "Zesty",
            };
            var fruit = new Fruit((0, 0), 1, details, 44)
            {
                Name = "Fluffy",
                Glyph = '*',
                Adjective = "So",
                Quantity = 120,
                Foreground = Color.BlanchedAlmond,
                IsUsable = false,
            };

            var json = JsonConvert.SerializeObject(fruit);

            var newFruit = JsonConvert.DeserializeObject<Fruit>(json);
            Assert.That(newFruit.Name, Is.EqualTo(fruit.Name));
            Assert.That(newFruit.Adjective, Is.EqualTo(fruit.Adjective));
            Assert.That(newFruit.Glyph, Is.EqualTo(fruit.Glyph));
            Assert.That(newFruit.Quantity, Is.EqualTo(fruit.Quantity));
            Assert.That(newFruit.Foreground, Is.EqualTo(fruit.Foreground));
            Assert.That(newFruit.IsUsable, Is.EqualTo(fruit.IsUsable));
            Assert.That(newFruit.ID, Is.EqualTo(44));
        }

        [Test]
        public void CRT_Knife()
        {
            var knife = new Knife((2, 2), id: 55)
            {
                Name = "Sharpy",
                Glyph = '-',
                Adjective = "Short",
                Quantity = 2,
                Foreground = Color.SteelBlue,
                IsUsable = false,
            };

            var json = JsonConvert.SerializeObject(knife);
            //if(!Debugger.IsAttached) Debugger.Launch();

            var newKnife = JsonConvert.DeserializeObject<Knife>(json);
            Assert.That(newKnife.Name, Is.EqualTo(knife.Name));
            Assert.That(newKnife.Adjective, Is.EqualTo(knife.Adjective));
            Assert.That(newKnife.Glyph, Is.EqualTo(knife.Glyph));
            Assert.That(newKnife.Quantity, Is.EqualTo(knife.Quantity));
            Assert.That(newKnife.Foreground, Is.EqualTo(knife.Foreground));
            Assert.That(newKnife.IsUsable, Is.EqualTo(knife.IsUsable));
            Assert.That(newKnife.ID, Is.EqualTo(55));
        }

        [Test]
        public void CRT_Seed()
        {
            Seed.Herbal.PlantByID[11] = new PlantDetails
                {ID = 11, FruitAdjective = "Fresh", SeedAdjective = "Burred", FruitKnown = true};

            var seed = new Seed((2, 2), 4, 11, id: 66)
            {
                Name = "Wendy",
                Glyph = '.',
                Adjective = "Burred",
                Quantity = 4,
                Foreground = Color.Tan,
                IsUsable = false,
            };

            var json = JsonConvert.SerializeObject(seed);
            //if(!Debugger.IsAttached) Debugger.Launch();

            var newSeed = JsonConvert.DeserializeObject<Seed>(json);
            Assert.That(newSeed.Name, Is.EqualTo(seed.Name));
            Assert.That(newSeed.Adjective, Is.EqualTo(seed.Adjective));
            Assert.That(newSeed.Glyph, Is.EqualTo(seed.Glyph));
            Assert.That(newSeed.Quantity, Is.EqualTo(seed.Quantity));
            Assert.That(newSeed.Foreground, Is.EqualTo(seed.Foreground));
            Assert.That(newSeed.IsUsable, Is.EqualTo(seed.IsUsable));
            Assert.That(newSeed.ID, Is.EqualTo(66));
        }

        [Test]
        public void CRT_Hoe()
        {
            var hoe = new Hoe((2, 2), id: 55)
            {
                Name = "Faithful",
                Glyph = '/',
                Adjective = "Copper",
                Quantity = 3,
                Foreground = Color.Red,
                IsUsable = true,
            };

            var json = JsonConvert.SerializeObject(hoe);
            //if(!Debugger.IsAttached) Debugger.Launch();

            var newHoe = JsonConvert.DeserializeObject<Hoe>(json);
            Assert.That(newHoe.Name, Is.EqualTo(hoe.Name));
            Assert.That(newHoe.Adjective, Is.EqualTo(hoe.Adjective));
            Assert.That(newHoe.Glyph, Is.EqualTo(hoe.Glyph));
            Assert.That(newHoe.Quantity, Is.EqualTo(hoe.Quantity));
            Assert.That(newHoe.Foreground, Is.EqualTo(hoe.Foreground));
            Assert.That(newHoe.IsUsable, Is.EqualTo(hoe.IsUsable));
            Assert.That(newHoe.ID, Is.EqualTo(55));
        }
    }
}
