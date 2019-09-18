using System;
using NUnit.Framework;
using NUnit.Framework.Internal;
using CopperBend.Fabric;

namespace sc.Tests
{
    [TestFixture]
    public class ClampedRatio_Tests
    {
        [TestCase(9, "1/3", 3)]
        [TestCase(1, "1/3", 0)]
        [TestCase(2, "1/3", 1)]
        [TestCase(1, "1/2", 1)]  // I chose to round the midpoint away from zero
        public void Can_apply_ratio(int input, string description, int expectedBlocked)
        {
            var cr = new ClampedRatio(description);
            int blocked = cr.Apply(input);
            Assert.That(blocked, Is.EqualTo(expectedBlocked));
        }

        [TestCase(9, "1/3 ..2", 2)]
        [TestCase(9, "1/3 ..", 3)]  // not useful, but not wrong
        [TestCase(9, "1/3 4..5", 4)]
        [TestCase(4, "2/3 4..", 4)]
        [TestCase(4, "2/3 8..", 8)]  // upper and lower bounds are hard, by default
        public void Can_clamp_ratio(int input, string description, int expectedBlocked)
        {
            var cr = new ClampedRatio(description);
            int blocked = cr.Apply(input);
            Assert.That(blocked, Is.EqualTo(expectedBlocked));
        }

        [TestCase(8, "2/1 ..6", 8)] // boost the effect of weak hits, but do not cap damage
        [TestCase(4, "2/3 8..", 4)] // even a good shield cannot block more damage than came in
        public void Can_let_input_move_clamp(int input, string description, int expectedBlocked)
        {
            var cr = new ClampedRatio(description);
            cr.InputMovesClamp = true;
            int blocked = cr.Apply(input);
            Assert.That(blocked, Is.EqualTo(expectedBlocked));
        }

        [TestCase(9, "1/3 +1", 4)]
        [TestCase(1, "1/3 +1", 1)]
        [TestCase(2, "1/3 +2", 3)]
        [TestCase(12, "1/3 -1", 3)]
        [TestCase(1, "1/3 -3", -3)] // could be negative.  If that's a worry, "0..".
        public void Can_offset_ratio(int input, string description, int expectedBlocked)
        {
            var cr = new ClampedRatio(description);
            int blocked = cr.Apply(input);
            Assert.That(blocked, Is.EqualTo(expectedBlocked));
        }

        [TestCase(9, "1/3 +3 2..4", 4)]
        [TestCase(1, "1/3 +1 2..4", 2)]
        [TestCase(12, "1/3 -2 4..5", 4)]
        public void Offset_is_applied_before_clamp(int input, string description, int expectedBlocked)
        {
            var cr = new ClampedRatio(description);
            int blocked = cr.Apply(input);
            Assert.That(blocked, Is.EqualTo(expectedBlocked));
        }

        [Test]
        public void ClampedRatio_complains_clearly_on_incorrect_notation()
        {
            var ex = Assert.Throws<FormatException>(() => new ClampedRatio("2d6"));
            Assert.That(ex.Message, Is.EqualTo("Cannot construct a clamped ratio with notation [2d6]."));
        }
    }
}
