using Kernelmethod.ChooseYourFighter;
using System;

namespace XRL.World.Parts {
    /// <summary>
    /// Custom part for the Magic Mirror object that allows the player to use it to change
    /// their appearance.
    /// </summary>
    [Serializable]
    public class Kernelmethod_ChooseYourFighter_MagicMirror : IPart {
        const string INVENTORY_COMMAND_ID = "Kernelmethod_ChooseYourFighter_Gaze";

        public override bool WantEvent(int ID, int cascade) {
            return base.WantEvent(ID, cascade)
                || ID == GetInventoryActionsEvent.ID
                || ID == InventoryActionEvent.ID;
        }

        public override bool HandleEvent(GetInventoryActionsEvent E) {
            if (E.Actor.IsPlayer())
                E.AddAction(
                    "Gaze",
                    Display: "gaze",
                    Command: INVENTORY_COMMAND_ID,
                    Key: 'g'
                );

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(InventoryActionEvent E) {
            if (E.Command == INVENTORY_COMMAND_ID && E.Actor.IsPlayer()) {
                var model = TileFactory.ChooseTileMenu();
                TileFactory.ChangePlayerAppearance(model);
                E.RequestInterfaceExit();
            }

            return base.HandleEvent(E);
        }
    }
}