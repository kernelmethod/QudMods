using HarmonyLib;
using XRL.CharacterBuilds.Qud;

namespace Kernelmethod.CrungleMode.Patches
{
    [HarmonyPatch(typeof(QudSubtypeModule), nameof(QudSubtypeModule.shouldBeEnabled))]
    public class QudSubtypeModulePatches : AbstractCharacterBuildModulePatch
    {
        static void Postfix(QudSubtypeModule __instance, ref bool __result)
        {
            __result = __result && (!InCrungleMode(__instance.builder));
        }
    }
}