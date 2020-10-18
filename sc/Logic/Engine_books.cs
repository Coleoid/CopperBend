using System;
using System.Text;
using CopperBend.Contract;
using CopperBend.Creation;
using CopperBend.Fabric;

namespace CopperBend.Logic
{
    /// <summary> This portion of the Engine handles Well-Knowns, in a book idiom. </summary>
    public partial class Engine
    {
        // INPROG:  Remove Cosmogenesis?  Does it become relevant again for stirring TomeOfChaos?
        public static void Cosmogenesis(string topSeed, ISadConEntityFactory factory = null)
        {
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
