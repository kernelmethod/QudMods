using HarmonyLib;
using XRL;
using XRL.World.Effects;

namespace Kernelmethod.IronMan.Patches {
    /// <summary>
    /// Harmony patches for the DeepDream.Crungle function that add the necessary parts for IronMan to the player's new body
    /// after being stricken by a dreamcrungle's crungling gaze.
    /// </summary>
    [HarmonyPatch(typeof(DeepDream), nameof(DeepDream.Crungle))]
    public class CrunglePatches
    {
        public static void Postfix()
        {
            The.Game.Player.Body.AddPart<SaveOnDeath>();
            The.Game.Player.Body.AddPart<SaveOnHealthThreshold>();
        }
    }
}