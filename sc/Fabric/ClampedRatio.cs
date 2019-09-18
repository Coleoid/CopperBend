using System;
using System.Text.RegularExpressions;

namespace CopperBend.Fabric
{
    public class ClampedRatio
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; }
        public int Offset { get; set; }
        public int UBound { get; set; }
        public int LBound { get; set; }
        
        // Set this true when, e.g., we cannot block more damage than came in
        public bool InputMovesClamp { get; set; }

        public ClampedRatio(int numerator, int denominator, int uBound = int.MaxValue, int lBound = int.MinValue, bool inputMovesClamp = false)
        {
            Numerator = numerator;
            Denominator = denominator;
            UBound = uBound;
            LBound = lBound;
            InputMovesClamp = inputMovesClamp;
        }

        public ClampedRatio(string description)
        {
            var match = Regex.Match(description, @"(\d+)/(\d+)(?: ([-+] ?\d+))?(?: (\d*)\.\.(\d*))?");
            if (!match.Success)
                throw new FormatException($"Cannot construct a clamped ratio with notation [{description}].");
            Numerator = int.Parse(match.Groups[1].Value);
            Denominator = int.Parse(match.Groups[2].Value);

            var offText = match.Groups[3].Value;
            Offset = string.IsNullOrEmpty(offText)
                ? 0
                : int.Parse(offText);

            var minText = match.Groups[4].Value;
            LBound = string.IsNullOrEmpty(minText)
                ? int.MinValue
                : int.Parse(minText);

            var maxText = match.Groups[5].Value;
            UBound = string.IsNullOrEmpty(maxText)
                ? int.MaxValue
                : int.Parse(maxText);
        }

        public int Apply(int input, MidpointRounding midpointRounding = MidpointRounding.AwayFromZero)
        {
            double raw = (input * Numerator) / (double) Denominator;
            int result = (int) Math.Round(raw, midpointRounding);
            result += Offset;

            var lBound = InputMovesClamp ? Math.Min(input, LBound) : LBound;
            var uBound = InputMovesClamp ? Math.Max(input, UBound) : UBound;

            result = Math.Max(result, lBound); // result is not lower than lower bound
            result = Math.Min(result, uBound); // result is not higher than upper bound

            return result;
        }
    }

    /*
     * 1. Apply pre-attack buffs
     * 2. Apply defensive methods
     *  dodge, parry/deflect
     *  block, armor
     * 3. Resolve to a set of effects (damage, ...)
     * 4. post-attack effects
     *  apply damage
     *  death, destruction, incapacitation
     *  morale, damage over time, ...
     *  use of resources
     *  experience
     *
     */
}
