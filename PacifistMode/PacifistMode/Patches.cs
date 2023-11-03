using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using XRL;
using XRL.Core;

namespace Kernelmethod.PermadeathWanderer.Patches
{
    [HarmonyPatch(typeof(WanderSystem), nameof(WanderSystem.PlayerEmbarking))]
    static class PlayerEmbarkingPatch {
        public static MethodInfo WANDER_ENABLED = AccessTools.Method(
            typeof(WanderSystem),
            nameof(WanderSystem.WanderEnabled)
        );

        public static bool WanderEnabledAndNotPermadeath() {
            var gameMode = XRLCore.Core.Game.GetStringGameState("GameMode");
            return WanderSystem.WanderEnabled()
                && gameMode != "Pacifist"
                && gameMode != "Permadeath Wander";
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code) {
            foreach (var op in code) {
                if (op.Calls(WANDER_ENABLED)) {
                    // Replace the call to WanderEnabled with our wrapper
                    yield return CodeInstruction.Call(
                        typeof(PlayerEmbarkingPatch),
                        nameof(WanderEnabledAndNotPermadeath)
                    );
                }
                else {
                    yield return op;
                }
            }
        }
    }
}
