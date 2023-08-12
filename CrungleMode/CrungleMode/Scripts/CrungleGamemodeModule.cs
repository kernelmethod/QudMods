using XRL.Core;
using XRL.UI;

namespace XRL.CharacterBuilds.Qud
{
    public class Kernelmethod_CrungleMode_CrungleGamemodeModule : QudGamemodeModule
    {
        public override bool shouldBeEnabled() {
            return builder?.GetModule<QudGamemodeModule>()?.GetMode() == "Kernelmethod_CrungleMode_CrungleMode";
        }

        public override void bootGame(XRLGame game, EmbarkInfo info) {
            if (GetMode() == "Roleplay")
                XRLCore.Core.Game.SetStringGameState("Checkpointing", "Enabled");
        }

        public override void Init() {
            // Temporarily override ShowQuickstartOption so that the game option
            // doesn't show up during selection.
            var oldOptionValue = Options.GetOption("OptionShowQuickstart");
            Options.SetOption("OptionShowQuickstart", "No");

            base.Init();

            // Reset ShowQuickstartOption to its old value
            Options.SetOption("OptionShowQuickstart", oldOptionValue);
        }
    }
}
