using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CopperBend.Contract;
using Troschuetz.Random.Generators;

namespace CopperBend.Fabric
{
    // The Tome of Chaos contains all the RNGs for the game.
    // We have multiple RNGs with their own responsibilities, to create
    // essentially repeatable worlds from an initial seed, regardless of
    // the path the player takes in the world.  This should improve debug
    // work, and demotivate savescumming to re-spin the wheel of treasure.
    public class TomeOfChaos : IBook
    {
        public string TopSeed { get; private set; }
        public int TopSeedInt { get; private set; }
        public AbstractGenerator TopGenerator { get; private set; }
        public AbstractGenerator MapGenerator { get; private set; }

        public TomeOfChaos()
            : this("must become better soon")
        { }

        public TomeOfChaos(string topSeed)
        {
            TopSeed = topSeed;
            using (MD5 md5Hasher = MD5.Create())
            {
                var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(TopSeed));
                TopSeedInt = BitConverter.ToInt32(hashed, 0);
            }
            TopGenerator = new XorShift128Generator(TopSeedInt);
            MapGenerator = new XorShift128Generator(TopGenerator.Next());
            MapPRNGS = new Dictionary<MapList, AbstractGenerator>();
            MapPRNGS[MapList.TackerFarm] = new XorShift128Generator(MapGenerator.Next());
            MapPRNGS[MapList.TownBastion] = new XorShift128Generator(MapGenerator.Next());
        }

        public string BookType { get; set; } = "TomeOfChaos";

        public int NextMapValue(MapList map)
        {
            return MapPRNG(map).Next();
        }

        private Dictionary<MapList, AbstractGenerator> MapPRNGS { get; set; }
        protected internal virtual AbstractGenerator MapPRNG(MapList map)
        {
            return MapPRNGS[map];
        }
    }


    public enum MapList
    {
        Unset = 0,
        TackerFarm,
        TownBastion,
    }

}
