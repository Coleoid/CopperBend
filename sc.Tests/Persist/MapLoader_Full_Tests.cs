using CopperBend.Fabric;
using NUnit.Framework;
using CopperBend.Model;

namespace CopperBend.Creation.Tests
{
    [TestFixture]
    public class MapLoader_Full_Tests : MapLoader_Tests_Base
    {
        private MapLoader _loader;

        [SetUp]
        public void SetUp()
        {
            var publisher = new BookPublisher(null);
            Atlas atlas = publisher.Atlas_FromNew();
            //var creator = new BeingCreator(__factory);
            //var ycb = new YConv_IBeing { BeingCreator = creator };
            _loader = new MapLoader(__log, atlas);
            Basis.ConnectIDGenerator();
        }

        //  The top-level purpose of the MapLoader
        [Test]
        public void CanGet_MapFromYAML()
        {
            var map = _loader.MapFromYAML(Ser_Test_Data.Get_RR_Yaml());

            Assert_Map_isRoundRoom(map);
        }

        [Test]
        public void CanGet_BridgeFromYAML()
        {
            var bridge = _loader.BridgeFromYAML(Ser_Test_Data.Get_RR_Yaml());
            Assert_Bridge_isRoundRoom(bridge);
        }

        [Test]
        public void CanGet_MapFromBridge()
        {
            var data = Ser_Test_Data.Get_RR_Bridge();

            var map = _loader.MapFromBridge(data);
            Assert_Map_isRoundRoom(map);
        }

        //[Test]
        //public void CanGet_Map_From_BigBadWolf_YAML()
        //{
        //    var data = _loader.BridgeFromYAML(SaveFileYaml);
        //    var map = _loader.MapFromBridge(data);
        //    Assert.That(map, Is.Not.Null);
        //}

        //  CanRT_* names:  Can Round-Trip an object to YAML and back.
        [Test]
        public void CanRT_Bridge_YAML_Bridge()
        {
            var rrBridge = Ser_Test_Data.Get_RR_Bridge();
            var yaml = _loader.YAMLFromBridge(rrBridge);
            var newBridge = _loader.BridgeFromYAML(yaml);
            Assert_Bridges_equiv(newBridge, rrBridge);
        }

        [Test]
        public void CanRT_Bridge_Map_Bridge()
        {
            var rrData = Ser_Test_Data.Get_RR_Bridge();
            var map = _loader.MapFromBridge(rrData);
            var data = _loader.BridgeFromMap(map);
            Assert_Bridges_equiv(data, rrData);
        }
    }
}
