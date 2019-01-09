using System;
using CopperBend.MapUtil;
using NUnit.Framework;
using System.Collections.Generic;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class PointTests
    {
        //  Earlier, I had Point as a class instead of a struct.
        //  Neither of these tests passed, reducing the usefulness of Point.

        [Test]
        public void Points_are_equal_when_XY_values_are_equal()
        {
            Point a = new Point(1, 2);
            Point b = new Point(1, 2);
            Point c = new Point(3, 4);

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
        }

        [Test]
        public void Points_as_keys_work_on_XY()
        {
            Point a = new Point(1, 2);
            Point b = new Point(1, 2);

            var dict = new Dictionary<Point, string>
            {
                [a] = "at A"
            };

            Assert.That(dict[b], Is.EqualTo("at A"));
        }

        [Test]
        public void DistanceTo()
        {
            Point origin = new Point(0, 0);
            Point target = new Point(1, 1);

            var distance = origin.DistanceTo(target);

            var sro2 = Math.Sqrt(2);
            Assert.That(distance, Is.EqualTo(sro2));
        }
    }
}
