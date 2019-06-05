using System.Collections.Generic;
using NUnit.Framework;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using CopperBend.Engine;
using CopperBend.Fabric;
using YamlDotNet.Serialization;
using YamlDotNet.RepresentationModel;

namespace sc_tests
{
    //  Throwing stuff at the wall  --  This is all mad dog prototyping

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
            smap.Add(new Space(), (0, 0));
            smap.Add(new Space(), (0, 1));
            smap.Add(new Space(), (1, 0));
            smap.Add(new Space(), (1, 1));

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

            Assert.Fail(yaml);
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
            Assert.Fail(yaml);
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
