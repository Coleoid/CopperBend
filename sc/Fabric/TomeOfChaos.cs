﻿using System;
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
        public AbstractGenerator TopGenerator { get; set; }
        
        public AbstractGenerator LearnableGenerator { get; set; }
        public AbstractGenerator MapTopGenerator { get; set; }
        private Dictionary<Maps, AbstractGenerator> MapGenerators { get; set; }


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
            LearnableGenerator = new XorShift128Generator(TopGenerator.Next());
            MapTopGenerator = new XorShift128Generator(TopGenerator.Next());
            MapGenerators = new Dictionary<Maps, AbstractGenerator>
            {
                [Maps.TackerFarm] = new XorShift128Generator(MapTopGenerator.Next()),
                [Maps.TownBastion] = new XorShift128Generator(MapTopGenerator.Next())
            };
        }

        protected internal virtual AbstractGenerator MapRndGen(Maps map)
        {
            return MapGenerators[map];
        }

        public int MapRndNext(Maps map)
        {
            return MapRndGen(map).Next();
        }

        public int LearnableRndNext()
        {
            return LearnableGenerator.Next();
        }
    }
}