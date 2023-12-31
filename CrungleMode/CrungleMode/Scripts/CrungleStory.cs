using ConsoleLib.Console;
using Qud.API;
using Qud.UI;
using UnityEngine;
using XRL.UI;

namespace XRL.World.Parts {
    public class Kernelmethod_CrungleMode_CrungleStory : IPart {
        public bool Triggered = false;

        public override bool SameAs(IPart p) {
            return false;
        }

        public override bool WantEvent(int ID, int cascade) {
            if (ID == BeforeTakeActionEvent.ID)
                return true;
            if (ID == ReplicaCreatedEvent.ID)
                return true;
            return base.WantEvent(ID, cascade);
        }

        public override bool HandleEvent(BeforeTakeActionEvent E) {
            if (!Triggered) {
                Triggered = true;

                string day = Calendar.getDay();
                string month = Calendar.getMonth();
                var name = "{{M|" + The.Player.DisplayName + "}}";
                var blueprint = "{{M|" + The.Player.GetBlueprint().DisplayName() + "}}";

                string text = "&yYou awaken from a fitful dream.\n\n";
                text += $"On the {day} of {month}, you enter the body of {name}, a {blueprint}.";
                string displayName = ParentObject.GetCurrentCell().ParentZone.DisplayName;

                if (CapabilityManager.AllowKeyboardHotkeys)
                    text += "\n\n<Press space, then press F1 for help.>";

                ClassicFade();

                // Create an initial checkpoint
                if (CheckpointingSystem.IsCheckpointingEnabled())
                    CheckpointingSystem.DoCheckpoint();

                Popup.Show(text);
                JournalAPI.AddAccomplishment(
                    $"On the {day} of {month}, you awoke from a fitful dream.",
                    $"On the terrible {day} of {month}, =name= entered a waking nightmare.",
                    // TODO: uncomment when beta gets merged into stable.
                    // muralCategory: MuralCategory.IsBorn,
                    // muralWeight: MuralWeight.Medium,
                    secretId: null,
                    time: -1
                );

                // Part is no longer needed
                ParentObject.RemovePart(this);
            }

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(ReplicaCreatedEvent E) {
            if (E.Object == ParentObject)
                E.WantToRemove(this);
            return base.HandleEvent(E);
        }

        public static void ClassicFade()
        {
            ScreenBuffer scrapBuffer = TextConsole.GetScrapBuffer1();
            scrapBuffer.Clear();
            scrapBuffer.Draw();
            FadeToBlack.FadeIn(0.5f, Color.black);
        }
    }
}
