using NUnit.Framework;
using RogueSharp;
using System.Collections.Generic;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class CoordinateTests
    {
        //  Earlier, I had Coord as a class.  
        //  Neither of these tests passed, reducing the usefulness of Coord.

        [Test]
        public void Comparing_coords_works_with_coords_as_structs()
        {
            Coord a = new Coord(1, 2);
            Coord b = new Coord(1, 2);
            Coord c = new Coord(3, 4);

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
        }

        [Test]
        public void Coords_as_Keys_work_through_interface()
        {
            Coord a = new Coord(1, 2);
            Coord b = new Coord(1, 2);

            var dict = new Dictionary<Coord, string>
            {
                [a] = "at A"
            };

            Assert.That(dict[b], Is.EqualTo("at A"));
        }
    }
}
