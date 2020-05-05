using System;
using System.Text;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Logic
{
    /// <summary> This portion of the Engine handles Well-Knowns, in a book idiom. </summary>
    public partial class Engine
    {
        public static Compendium Compendium { get; set; }
        public static BeingCreator BeingCreator { get => Compendium.BeingCreator; }

        public static void Cosmogenesis(string topSeed, ISadConEntityFactory factory)
        {
            var generator = new IDGenerator();
            ConnectIDGenerator(generator);

            var creator = new BeingCreator(factory);

            var publisher = new BookPublisher(creator);

            var tome = publisher.Tome_FromNew(topSeed);

            var herbal = publisher.Herbal_FromNew();
            ConnectHerbal(herbal);

            var register = publisher.Register_FromNew(creator);
            ConnectSocialRegister(register);

            var dramaticon = publisher.Dramaticon_FromNew();

            var atlas = publisher.Atlas_FromNew();

            Compendium = new Compendium(generator, creator, tome, herbal, register, dramaticon, atlas);
        }

        public static void ConnectIDGenerator(IDGenerator gen)
        {
            CbEntity.SetIDGenerator(gen);
            Item.SetIDGenerator(gen);
            Space.SetIDGenerator(gen);
            AreaRot.SetIDGenerator(gen);
        }

        public static void ConnectHerbal(Herbal herbal)
        {
            Equipper.Herbal = herbal;
        }

        public static void ConnectSocialRegister(SocialRegister register)
        {
            var pc = register.CreatePlayer();
            register.LoadRegister(pc);
        }

        private static string GenerateSimpleTopSeed()
        {
            string clearLetters = "bcdefghjkmnpqrstvwxyz";
            var r = new Random();
            var b = new StringBuilder();
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append('-');
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append('-');
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append(clearLetters[r.Next(0, 20)]);

            return b.ToString();
        }
    }
}
