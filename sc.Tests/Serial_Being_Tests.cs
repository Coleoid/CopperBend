using System.Linq;
using CopperBend.Contract;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NUnit.Framework;
using CopperBend.Model;
using CopperBend.Persist;
using NSubstitute;

namespace sc_tests
{
    [TestFixture]
    public class Serial_Being_Tests
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

            Being.EntityFactory = Substitute.For<IEntityFactory>();
        }

        [Test]
        public void CRT_Being()
        {
            var being = new Being(Color.Bisque, Color.Azure, 'g', 77);
            var json = JsonConvert.SerializeObject(being);
            var newBeing = JsonConvert.DeserializeObject<Being>(json);
            Assert.That(newBeing.Name, Is.EqualTo(being.Name));
            Assert.That(newBeing.Foreground, Is.EqualTo(being.Foreground));
            Assert.That(newBeing.Background, Is.EqualTo(being.Background));
            Assert.That(newBeing.Glyph, Is.EqualTo(being.Glyph));
            Assert.That(newBeing.ID, Is.EqualTo(being.ID));
        }

        [Test]
        public void CRT_Monster()
        {
            var being = new Monster(Color.Bisque, Color.Azure, 'g', 33);
            var json = JsonConvert.SerializeObject(being);
            var newBeing = JsonConvert.DeserializeObject<IBeing>(json);
            Assert.That(newBeing, Is.TypeOf<Monster>());
            Assert.That(newBeing.Name, Is.EqualTo(being.Name));
            Assert.That(newBeing.Foreground, Is.EqualTo(being.Foreground));
            Assert.That(newBeing.Background, Is.EqualTo(being.Background));
            Assert.That(newBeing.Glyph, Is.EqualTo(being.Glyph));
            Assert.That(newBeing.ID, Is.EqualTo(being.ID));
        }

        [Test]
        public void CRT_Player()
        {
            var being = new Player(Color.Green, Color.Black, '@', 44);
            var json = JsonConvert.SerializeObject(being);
            var newBeing = JsonConvert.DeserializeObject<IBeing>(json);
            Assert.That(newBeing, Is.TypeOf<Player>());
            var newPlayer = (Player)newBeing;
            Assert.That(newPlayer.Name, Is.EqualTo(being.Name));
            Assert.That(newPlayer.Foreground, Is.EqualTo(being.Foreground));
            Assert.That(newPlayer.Background, Is.EqualTo(being.Background));
            Assert.That(newPlayer.Glyph, Is.EqualTo(being.Glyph));
            Assert.That(newPlayer.ID, Is.EqualTo(being.ID));
        }

        [Test]
        public void CRT_Player_inventory()
        {
            var being = new Player(Color.Green, Color.Black, '@', 44);
            being.AddToInventory(new Knife((3,3), id: 17));
            being.AddToInventory(new Hoe((1,1), id: 23));
            var json = JsonConvert.SerializeObject(being);
            var newBeing = JsonConvert.DeserializeObject<IBeing>(json);
            Assert.That(newBeing, Is.TypeOf<Player>());
            var newPlayer = (Player)newBeing;
            var inv = newPlayer.Inventory.ToList();
            Assert.That(inv.Count, Is.EqualTo(2));
            Assert.That(inv[0].ID, Is.EqualTo(17));
            Assert.That(inv[0].Location, Is.EqualTo((3,3)));
            Assert.That(inv[0], Is.TypeOf<Knife>());
            Assert.That(inv[1].ID, Is.EqualTo(23));
            Assert.That(inv[1].ItemType, Is.EqualTo("Hoe"));
        }
    }
}
