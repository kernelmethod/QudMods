using HarmonyLib;
using Kernelmethod.KernelSpace;
using XRL.World;

namespace Kernelmethod.KernelSpace.Patches {
    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
    public static class SoundManagerPlaySoundPatch {
        public static bool Prefix(string Clip) {
            if (!Options.DisableEatingSounds)
                return true;

            switch (Clip) {
                case "Human_Eating":
                    // Don't play the sound
                    return false;
                default:
                    return true;
            }
        }
    }
}
