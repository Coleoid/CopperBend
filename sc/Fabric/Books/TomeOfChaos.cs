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
        public string BookType { get => "TomeOfChaos"; }

        public string TopSeed { get; private set; }
        public int TopSeedInt { get; private set; }
        public Dictionary<string, AbstractGenerator> Generators { get; }
        public Dictionary<MapEnum, AbstractGenerator> MapGenerators { get; }


        public TomeOfChaos()
        {
            Generators = new Dictionary<string, AbstractGenerator>();
            MapGenerators = new Dictionary<MapEnum, AbstractGenerator>();
        }

        public void SetTopSeed(string topSeed)
        {
            TopSeed = topSeed;

            using SHA256 shaHasher = SHA256.Create();
            var hashed = shaHasher.ComputeHash(Encoding.UTF8.GetBytes(TopSeed));
            TopSeedInt = BitConverter.ToInt32(hashed, 0);
        }

        public int LearnableRndNext() => Generators["Learnable"].Next();
        public int MapRndNext(MapEnum map) => MapGenerators[map].Next();
    }
}
