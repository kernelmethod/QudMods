using XRL;
using XRL.Core;
using XRL.World;

namespace Kernelmethod.VillageFinder
{
    [PlayerMutator]
    public class VillageFinderPlayerMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            player.AddPart<EncounterModifier>();
        }
    }

    [HasCallAfterGameLoadedAttribute]
    public class AddPartsToPlayerHandler
    {
        [CallAfterGameLoadedAttribute]
        public static void AddPartsCallback()
        {
            GameObject player = XRLCore.Core?.Game?.Player?.Body;

            if (player == null)
                return;

            player.RequirePart<EncounterModifier>();
        }
    }
}
