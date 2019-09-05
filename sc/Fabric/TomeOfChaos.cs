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
        public string BookType { get; set; } = "TomeOfChaos";

        public string TopSeed { get; private set; }
        public int TopSeedInt { get; private set; }
        public AbstractGenerator TopGenerator { get; private set; }
        
        public AbstractGenerator MapTopGenerator { get; private set; }
        private Dictionary<Maps, AbstractGenerator> MapGenerators { get; set; }

        public AbstractGenerator LearnableTopGenerator { get; private set; }
        private Dictionary<Learnables, AbstractGenerator> LearnableGenerators { get; set; }

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

            // The order of each group of 'new ...Next())' calls matters for
            // repeatability, which affects saved games and debug dumps.
            MapTopGenerator = new XorShift128Generator(TopGenerator.Next());
            LearnableTopGenerator = new XorShift128Generator(TopGenerator.Next());

            MapGenerators = new Dictionary<Maps, AbstractGenerator>();
            MapGenerators[Maps.TackerFarm] = new XorShift128Generator(MapTopGenerator.Next());
            MapGenerators[Maps.TownBastion] = new XorShift128Generator(MapTopGenerator.Next());

            LearnableGenerators = new Dictionary<Learnables, AbstractGenerator>();
            LearnableGenerators[Learnables.Seed] = new XorShift128Generator(LearnableTopGenerator.Next());
            LearnableGenerators[Learnables.Fruit] = new XorShift128Generator(LearnableTopGenerator.Next());
            LearnableGenerators[Learnables.Potion] = new XorShift128Generator(LearnableTopGenerator.Next());
            LearnableGenerators[Learnables.Scroll] = new XorShift128Generator(LearnableTopGenerator.Next());
        }

        protected internal virtual AbstractGenerator MapRndGen(Maps map)
        {
            return MapGenerators[map];
        }

        public int MapRndNext(Maps map)
        {
            return MapRndGen(map).Next();
        }

        protected internal virtual AbstractGenerator LearnableRndGen(Learnables Learnable)
        {
            return LearnableGenerators[Learnable];
        }

        public int LearnableRndNext(Learnables Learnable)
        {
            return LearnableRndGen(Learnable).Next();
        }
    }
}
