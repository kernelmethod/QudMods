using XRL;
using XRL.Core;
using XRL.World;

namespace Kernelmethod.PrecogSecrets {
    [PlayerMutator]
    public class PrecogHandlerMutator : IPlayerMutator {
        /// <summary>
        /// Add the PrecogSecretHandler part to the player on starting a new game.
        /// </summary>
        public void mutate(GameObject player) {
            player.AddPart<PrecogSecretHandler>();
        }
    }

    [HasCallAfterGameLoadedAttribute]
    public class AddPrecogSecretHandlerToPlayer
    {
        [CallAfterGameLoadedAttribute]
        public static void AddPrecogSecretHandlerCallback()
        {
            GameObject player = XRLCore.Core?.Game?.Player?.Body;
            if (player == null)
                return;

            player.RequirePart<PrecogSecretHandler>();
        }
    }
}
