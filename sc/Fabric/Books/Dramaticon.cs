using CopperBend.Contract;

namespace CopperBend.Fabric
{
    // Stories, scenes, quests, visions, dreams
    // currently containing state later to be split out to StoryLog or Diary or such
    public class Dramaticon : IBook
    {
        public bool HasClearedRot { get; set; }
    }
}
