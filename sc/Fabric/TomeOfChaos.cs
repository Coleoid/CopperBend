using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Troschuetz.Random.Generators;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    // The Tome of Chaos contains all the RNGs for the game.
    // We have multiple RNGs with their own responsibilities, to create
    // essentially repeatable worlds from an initial seed, regardless of
    // the path the player takes in the world.  This should improve debug
    // work, and demotivate savescumming to re-spin the wheel of treasure.
    public class TomeOfChaos : IBook
    {
        public string BookType { get; set; } = "TomeOfChaos";

        public string TopSeed { get; private set; }
        public int TopSeedInt { get; private set; }
        public AbstractGenerator TopGenerator { get; set; }

        public AbstractGenerator LearnableGenerator { get; set; }
        public AbstractGenerator MapTopGenerator { get; set; }
        public Dictionary<Maps, AbstractGenerator> MapGenerators { get; }


        public TomeOfChaos(string topSeed)
        {
            TopSeed = topSeed;
            MapGenerators = new Dictionary<Maps, AbstractGenerator>();

            using (SHA256 shaHasher = SHA256.Create())
            {
                var hashed = shaHasher.ComputeHash(Encoding.UTF8.GetBytes(TopSeed));
                TopSeedInt = BitConverter.ToInt32(hashed, 0);
            }
            TopGenerator = new NR3Generator(TopSeedInt);

            // The order of calls to a generator matters for
            // repeatability, which affects saved games and debug dumps.
            LearnableGenerator = new NR3Generator(TopGenerator.Next());
            MapTopGenerator = new NR3Generator(TopGenerator.Next());
        }

        public int LearnableRndNext()
        {
            return LearnableGenerator.Next();
        }

        public int MapRndNext(Maps map)
        {
            return MapRndGen(map).Next();
        }

        protected internal virtual AbstractGenerator MapRndGen(Maps map)
        {
            return MapGenerators[map];
        }
    }
}
