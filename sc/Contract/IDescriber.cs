using CopperBend.Fabric;

namespace CopperBend.Contract
{
    public interface IDescriber
    {
        string Describe(IItem item, DescMods mods = DescMods.None);
        string Describe(string name, DescMods mods = DescMods.None, int quantity = 1, string adj = "");
        void Scramble();

        //0.1 causes upward ref
        TomeOfChaos TomeOfChaos { get; set; }
    }
}
