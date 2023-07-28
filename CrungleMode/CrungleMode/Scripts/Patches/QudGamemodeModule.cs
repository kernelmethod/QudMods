using HarmonyLib;
using XRL.CharacterBuilds.Qud;

namespace Kernelmethod.CrungleMode.Patches
{
    [HarmonyPatch(typeof(QudGamemodeModule), nameof(QudGamemodeModule.SelectMode))]
    public class QudGamemodeModulePatches : AbstractCharacterBuildModulePatch
    {
        static void Postfix(QudGameBootModule __instance)
        {
            if (__instance.builder == null)
                return;
            if (!InCrungleMode(__instance.builder))
                return;
    
            // Get rid of genotype information
            if (__instance.builder.GetModule<QudGenotypeModule>() != null)
                __instance.builder.GetModule<QudGenotypeModule>().data = null;

            
        }
    }
}