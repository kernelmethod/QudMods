using XRL;
using XRL.Core;
using XRL.World;

using Kernelmethod.IronMan.Parts;

namespace Kernelmethod.IronMan
{

    [PlayerMutator]
    public class SaveMutator : IPlayerMutator
    {
        public static bool ApplicableGameState() {
            var gameMode = The.Game.GetStringGameState("GameMode");
            return gameMode == "Classic" || gameMode == "_Quickstart";
        }

        public void mutate(GameObject player)
        {
            if (!ApplicableGameState())
                return;

            player.AddPart<IronManSavePart>();
        }
    }

    [HasCallAfterGameLoadedAttribute]
    public class AddSavePartsToPlayerHandler
    {
        [CallAfterGameLoadedAttribute]
        public static void AddPartsCallback()
        {
            GameObject player = XRLCore.Core?.Game?.Player?.Body;

            if (player == null || !SaveMutator.ApplicableGameState())
                return;

            player.RequirePart<IronManSavePart>();
        }
    }
}
