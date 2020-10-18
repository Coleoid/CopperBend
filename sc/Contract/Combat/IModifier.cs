namespace CopperBend.Contract
{
    public interface IModifier
    {
        /// <summary>
        /// Some modifiers are expended, like "blocks a total of 80 damage" or
        /// "Sharpness enchantment wears off after five hits".  Used modifiers
        /// get WasUsed(amount) calls that they do what they want with.
        /// </summary>
        /// <param name="amount">how much effect the modifier had</param>
        void WasUsed(int amount);
    }
}
