using CopperBend.Contract;
using GoRogue;

namespace CopperBend.Fabric
{
    public class Compendium : IBook
    {
        public IDGenerator IDGenerator { get; }
        public BeingCreator BeingCreator { get; }
        public TomeOfChaos TomeOfChaos { get; }
        public Herbal Herbal { get; }
        public SocialRegister SocialRegister { get; }
        public Dramaticon Dramaticon { get; }
        public Atlas Atlas { get; }

        public Compendium(
            IDGenerator idGen,
            BeingCreator beingCreator,
            TomeOfChaos tomeOfChaos,
            Herbal herbal,
            SocialRegister socialRegister,
            Dramaticon dramaticon,
            Atlas atlas
        )
        {
            IDGenerator = idGen;
            BeingCreator = beingCreator;
            TomeOfChaos = tomeOfChaos;
            Herbal = herbal;
            SocialRegister = socialRegister;
            Dramaticon = dramaticon;
            Atlas = atlas;
        }
    }
}
