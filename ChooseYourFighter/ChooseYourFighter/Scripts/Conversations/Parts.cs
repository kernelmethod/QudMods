using Kernelmethod.ChooseYourFighter.UI;

namespace XRL.World.Conversations.Parts {
    public class Kernelmethod_ChooseYourFighter_ChangeAppearance : IConversationPart {
        /// <summary>
        /// Cost (in drams) to change appearance.
        /// </summary>
        public const int ChangeAppearanceCost = 16;

        public override bool WantEvent(int ID, int propagation) {
            return ID == EnterElementEvent.ID
                || base.WantEvent(ID, propagation);
        }

        public override bool HandleEvent(EnterElementEvent E) {
            TileMenu.ChangeAppearanceMenu();
            return base.HandleEvent(E);
        }
    }
}
