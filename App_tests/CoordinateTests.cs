using CopperBend.MapUtil;
using NUnit.Framework;
using System.Collections.Generic;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class PointinateTests
    {
        //  Earlier, I had Point as a class.  
        //  Neither of these tests passed, reducing the usefulness of Point.

        [Test]
        public void Comparing_coords_works_with_coords_as_structs()
        {
            Point a = new Point(1, 2);
            Point b = new Point(1, 2);
            Point c = new Point(3, 4);

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
        }

        [Test]
        public void Point_as_Keys_work_through_interface()
        {
            Point a = new Point(1, 2);
            Point b = new Point(1, 2);

            var dict = new Dictionary<Point, string>
            {
                [a] = "at A"
            };

            Assert.That(dict[b], Is.EqualTo("at A"));
        }
    }
}
