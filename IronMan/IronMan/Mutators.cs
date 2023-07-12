using XRL;
using XRL.Core;
using XRL.World;

namespace Kernelmethod.IronMan
{

    [PlayerMutator]
    public class SaveMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            // Only add parts to the player if the game mode is Classic
            if (The.Game.GetStringGameState("GameMode", "Classic") == "Classic")
            {
                player.AddPart<SaveOnDeath>();
                player.AddPart<SaveOnHealthThreshold>();
            }
        }
    }

    [HasCallAfterGameLoadedAttribute]
    public class AddSavePartsToPlayerHandler
    {
        [CallAfterGameLoadedAttribute]
        public static void AddSaveOnDeathCallback()
        {
            GameObject player = XRLCore.Core?.Game?.Player?.Body;

            if (player != null && The.Game.GetStringGameState("GameMode", "Classic") == "Classic")
            {
                player.RequirePart<SaveOnDeath>();
                player.RequirePart<SaveOnHealthThreshold>();
            }
        }
    }
}