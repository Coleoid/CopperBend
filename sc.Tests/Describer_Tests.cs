using GoRogue;
using CopperBend.Model;
using NUnit.Framework;
using CopperBend.Contract;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class Describer_Tests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Engine.Cosmogenesis("seed");
        }

        [TestCase(1, "", "rock")]
        [TestCase(2, "", "rocks")]
        [TestCase(2, "igneous", "igneous rocks")]
        [TestCase(1, "grey", "grey rock")]
        public void Describe_without_options(int quantity, string adjective, string expected)
        {
            var describer = new Describer();

            var item = new Item(new Coord(0, 0), quantity, false)
            {
                Name = "rock",
                Adjective = adjective,
            };
            var desc = describer.Describe(item, DescMods.None);
            Assert.That(desc, Is.EqualTo(expected));
        }

        [TestCase(1, "", "the rock")]
        [TestCase(1, "grey", "the grey rock")]
        [TestCase(2, "", "the rocks")]
        [TestCase(2, "igneous", "the igneous rocks")]
        public void Describe_with_definite_article(int quantity, string adjective, string expected)
        {
            var describer = new Describer();

            var item = new Item(new Coord(0, 0), quantity, false)
            {
                Name = "rock",
                Adjective = adjective,
            };
            var desc = describer.Describe(item, DescMods.Article | DescMods.Definite);
            Assert.That(desc, Is.EqualTo(expected));
        }

        [TestCase(1, "", "a rock")]
        [TestCase(1, "grey", "a grey rock")]
        [TestCase(1, "icy", "an icy rock")]
        [TestCase(2, "", "some rocks")]
        [TestCase(2, "igneous", "some igneous rocks")]
        public void Describe_with_indefinite_article(int quantity, string adjective, string expected)
        {
            var describer = new Describer();

            var item = new Item((0, 0), quantity, false)
            {
                Name = "rock",
                Adjective = adjective,
            };
            var desc = describer.Describe(item, DescMods.Article);
            Assert.That(desc, Is.EqualTo(expected));
        }

        [TestCase(1, "", "1 rock")]
        [TestCase(1, "grey", "1 grey rock")]
        [TestCase(2, "", "2 rocks")]
        [TestCase(2, "igneous", "2 igneous rocks")]
        public void Describe_with_quantity(int quantity, string adjective, string expected)
        {
            var describer = new Describer();

            var item = new Item((0, 0), quantity, false)
            {
                Name = "rock",
                Adjective = adjective,
            };
            var desc = describer.Describe(item, DescMods.Quantity);
            Assert.That(desc, Is.EqualTo(expected));
        }

        [TestCase(1, "", "a rock")]
        [TestCase(1, "icy", "an icy rock")]
        [TestCase(2, "", "2 rocks")]
        [TestCase(2, "igneous", "2 igneous rocks")]
        public void Describe_with_quantity_and_indef_art(int quantity, string adjective, string expected)
        {
            var describer = new Describer();

            var item = new Item((0, 0), quantity, false)
            {
                Name = "rock",
                Adjective = adjective,
            };
            var desc = describer.Describe(item, DescMods.Quantity | DescMods.Article);
            Assert.That(desc, Is.EqualTo(expected));
        }

        [TestCase(1, "", "Rock")]
        [TestCase(1, "icy", "Icy rock")]
        [TestCase(2, "", "Rocks")]
        [TestCase(2, "igneous", "Igneous rocks")]
        public void Describe_with_leading_capital(int quantity, string adjective, string expected)
        {
            var describer = new Describer();

            var item = new Item((0, 0), quantity, false)
            {
                Name = "rock",
                Adjective = adjective,
            };
            var desc = describer.Describe(item, DescMods.LeadingCapital);
            Assert.That(desc, Is.EqualTo(expected));
        }

        [TestCase(1, "", "rock")]
        [TestCase(1, "icy", "rock")]
        [TestCase(2, "", "rocks")]
        [TestCase(2, "igneous", "rocks")]
        public void Describe_with_no_adjective(int quantity, string adjective, string expected)
        {
            var describer = new Describer();

            var item = new Item((0, 0), quantity, false)
            {
                Name = "rock",
                Adjective = adjective,
            };
            var desc = describer.Describe(item, DescMods.NoAdjective);
            Assert.That(desc, Is.EqualTo(expected));
        }

        //[Test]
        //public void Describe_a_seed_unknown_and_known()
        //{
        //    var describer = new Describer(87);

        //    var item = new Seed((0, 0), 1, PlantType.Healer);

        //    var desc = describer.Describe(item);
        //    Assert.That(desc, Is.EqualTo("rough seed"));

        //    describer.Learn(item);

        //    desc = describer.Describe(item);
        //    Assert.That(desc, Is.EqualTo("healer seed"));
        //}

        //[Test]
        //public void Describe_a_fruit_unknown_and_known()
        //{
        //    var describer = new Describer(87);

        //    var item = new Fruit((0, 0), 1, PlantType.Healer);

        //    var desc = describer.Describe(item);
        //    Assert.That(desc, Is.EqualTo("star-shaped fruit"));

        //    describer.Learn(item);

        //    desc = describer.Describe(item);
        //    Assert.That(desc, Is.EqualTo("healer fruit"));
        //}

        [TestCase("apple", true)]
        [TestCase("Bear", false)]
        [TestCase("Electron", true)]
        [TestCase("imp", true)]
        [TestCase("keen gaze", false)]
        [TestCase("only move", true)]
        [TestCase("yellow star", false)] // leading Y is consonant, 99% +
        [TestCase("understanding", true)]
        [TestCase("imp", true)]
        public void HasLVS_normal_cases(string word, bool hasLeadingVowelSound)
        {
            Assert.That(Describer.HasLeadingVowelSound(word), Is.EqualTo(hasLeadingVowelSound));
        }

        [TestCase("universe")]
        [TestCase("unit")]
        [TestCase("one")]
        [TestCase("use")]
        [TestCase("user")]
        [TestCase("utensil")]
        [TestCase("ewe")]
        [TestCase("ewer")]
        [TestCase("euphoric")]
        public void HasLVS_vowel_with_consonant_sound(string word)
        {
            Assert.That(Describer.HasLeadingVowelSound(word), Is.False);
        }

        [TestCase("yttrium")]
        [TestCase("Ymir")]
        [TestCase("honest child")]
        [TestCase("herbal remedy")]
        [TestCase("heir")]
        public void HasLVS_consonant_with_vowel_sound(string word)
        {
            Assert.That(Describer.HasLeadingVowelSound(word));
        }

        [TestCase("unimportant", true)]
        [TestCase("honey", false)]
        [TestCase("hierarchy", false)]
        public void HasLVS_normal_but_similar_to_unusual(string word, bool hasLeadingVowelSound)
        {
            Assert.That(Describer.HasLeadingVowelSound(word), Is.EqualTo(hasLeadingVowelSound));
        }

    }
}
