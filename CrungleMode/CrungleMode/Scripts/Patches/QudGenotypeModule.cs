using HarmonyLib;
using XRL.CharacterBuilds.Qud;

namespace Kernelmethod.CrungleMode.Patches
{
    [HarmonyPatch(typeof(QudGenotypeModule), nameof(QudGenotypeModule.shouldBeEnabled))]
    public class QudGenotypeModulePatches : AbstractCharacterBuildModulePatch
    {
        static void Postfix(QudGenotypeModule __instance, ref bool __result)
        {
            __result = __result && (!InCrungleMode(__instance.builder));
        }
    }
}