using HarmonyLib;
using XRL.CharacterBuilds.Qud;

namespace Kernelmethod.CrungleMode.Patches
{
    [HarmonyPatch(typeof(QudChartypeModule), nameof(QudChartypeModule.shouldBeEnabled))]
    public class QudChartypeModulePatches : AbstractCharacterBuildModulePatch
    {
        static void Postfix(QudChartypeModule __instance, ref bool __result)
        {
            __result = __result && (!InCrungleMode(__instance.builder));
        }
    }
}