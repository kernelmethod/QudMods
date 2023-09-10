using XRL;
using XRL.Liquids;
using XRL.UI;
using Kernelmethod.ChooseYourFighter;

namespace XRL.World.Conversations.Parts {
    public class Kernelmethod_ChooseYourFighter_ChangeAppearance : IConversationPart {
        public BaseLiquid Liquid = new LiquidWater();

        /// <summary>
        /// Cost (in drams) to change appearance.
        /// </summary>
        public const int ChangeAppearanceCost = 16;

        public override bool WantEvent(int ID, int propagation) {
            return ID == GetChoiceTagEvent.ID
                || ID == EnterElementEvent.ID
                || base.WantEvent(ID, propagation);
        }

        public override bool HandleEvent(GetChoiceTagEvent E) {
            E.Tag += $"{{{{g|[{ChangeAppearanceCost} drams of {Liquid.GetName()}]}}}}";
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(EnterElementEvent E) {
            if (The.Player.GetFreeDrams(Liquid.ID) < ChangeAppearanceCost) {
                Popup.ShowFail($"You don't have enough {Liquid.GetName()}.");
                return base.HandleEvent(E);
            }

            var model = TileFactory.ChooseTileMenu();
            if (model == null)
                return base.HandleEvent(E);

            TileFactory.ChangePlayerAppearance(model);

            // Charge the player at the end
            The.Player.UseDrams(ChangeAppearanceCost, Liquid.ID);
            return base.HandleEvent(E);
        }
    }
}
