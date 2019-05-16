using NUnit.Framework;
using GoRogue;

namespace sc_tests
{
    [TestFixture]
    public class Serialization_tests
    {
        [Test]
        public void IDGen_output()
        {
            var gen = new IDGenerator();
            for (var i = 0; i < 10; i++)
            {
                var id = gen.UseID();
            }
        }
    }
}
