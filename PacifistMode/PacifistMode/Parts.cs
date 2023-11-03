using XRL;
using XRL.World;
using XRL.World.Parts;
using XRL.UI;

namespace Kernelmethod.PacifistMode.Parts {
    public class PacifistModeConductChecker : IPlayerPart {
        public override bool WantEvent(int ID, int Cascade)
        {
            return base.WantEvent(ID, Cascade)
                || ID == KilledEvent.ID;
        }

        public override bool HandleEvent(KilledEvent E)
        {
            var killer = E.Killer;
            if (killer == null || !killer.IsPlayerControlled())
                return base.HandleEvent(E);

            if (E.Dying == null || E.Dying.HasTag("Kernelmethod_PacifistMode_OkayToKill"))
                return base.HandleEvent(E);

            Popup.Show("You broke your covenant!");
            Popup.Show("Your head explodes!");
            var notPlayer = killer.IsPlayer();
            killer.BigBloodsplatter();
            killer.TakeDamage(
                killer.hitpoints,
                "from breaking %t sacred covenant!",
                Attributes: "Unavoidable",
                DeathReason: "You were smited by Shekinah."
            );

            if (notPlayer) {
                The.Player.BigBloodsplatter();
                The.Player.TakeDamage(
                    The.Player.hitpoints,
                    "from breaking %t sacred covenant!",
                    Attributes: "Unavoidable",
                    DeathReason: "You were smited by Shekinah."
                );
            }

            return base.HandleEvent(E);
        }
    }
}
