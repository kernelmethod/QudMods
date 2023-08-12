using HarmonyLib;
using XRL.Core;

namespace Kernelmethod.CrungleMode.Patches
{
    [HarmonyPatch(typeof(Scoreboard2), nameof(Scoreboard2.Add))]
    public class Scoreboard2Patches : AbstractCharacterBuildModulePatch
    {
        static bool Prefix(Scoreboard2 __instance, int Score, string Details, long Turn, string GameId, string GameMode, bool ReplaceOnId = false, int Level = 0, string Name = "")
        {
            return GameMode != "Crungle Mode!" || Options.SaveScores;
        }
    }
}
