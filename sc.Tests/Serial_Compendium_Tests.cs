using CopperBend.Contract;
using Newtonsoft.Json;
using NUnit.Framework;
using CopperBend.Persist;
using CopperBend.Fabric;

namespace sc_tests
{
    [TestFixture]
    public class Serial_Compendium_Tests
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
        public void CRT_Tome_of_Chaos()
        {
            var tome = new TomeOfChaos();
            var json = JsonConvert.SerializeObject(tome);
            var newTome = JsonConvert.DeserializeObject<IBook>(json);
            Assert.That(newTome, Is.TypeOf<TomeOfChaos>());
        }
    }

}
