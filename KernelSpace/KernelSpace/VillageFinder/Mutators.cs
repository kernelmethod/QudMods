using XRL;
using XRL.Core;
using XRL.World;

namespace Kernelmethod.KernelSpace.Parts
{
    [PlayerMutator]
    public class VillageFinderPlayerMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            player.AddPart<VillageFinder>();
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

            player.RequirePart<VillageFinder>();
        }
    }
}
