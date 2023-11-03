using XRL;
using XRL.Core;
using XRL.World;
using Kernelmethod.PacifistMode.Parts;

namespace Kernelmethod.PacifistMode
{

    [PlayerMutator]
    public class SaveMutator : IPlayerMutator
    {
        const string GAME_MODE = "Pacifist";

        public static bool ApplicableGameMode() {
            return The.Game.GetStringGameState("GameMode") == GAME_MODE;
        }

        public void mutate(GameObject player)
        {
            if (ApplicableGameMode())
                player.AddPart<PacifistModeConductChecker>();
        }
    }

    [HasCallAfterGameLoadedAttribute]
    public class AddSavePartsToPlayerHandler
    {
        [CallAfterGameLoadedAttribute]
        public static void AddPartsCallback()
        {
            if (!SaveMutator.ApplicableGameMode())
                return;
            GameObject player = XRLCore.Core?.Game?.Player?.Body;
            player?.RequirePart<PacifistModeConductChecker>();        }
    }
}
