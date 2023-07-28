using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;

namespace Kernelmethod.CrungleMode.Patches {
    public abstract class AbstractCharacterBuildModulePatch {
        public static bool InCrungleMode(EmbarkBuilder builder) {
            return builder?.GetModule<QudGamemodeModule>()?.GetMode() == "Kernelmethod_CrungleMode_CrungleMode";
        }
    }
}