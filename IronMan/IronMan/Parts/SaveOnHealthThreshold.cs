using System;
using XRL.World;

namespace Kernelmethod.IronMan
{
    /// <summary>
    /// Custom part that triggers a save whenever the player's health drops below a certain threshold.
    /// </summary>
    [Serializable]
    public class SaveOnHealthThreshold : AbstractSavePart
    {
        public override long MinTurnsBetweenSaves => 300;
        public double HealthThreshold = 0.4;

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == BeforeTookDamageEvent.ID;
        }

        public override bool HandleEvent(BeforeTookDamageEvent E)
        {
            if (!ParentObject.IsPlayer())
                goto Exit;

            // Check whether the damage we're about to take puts us below the threshold
            // (but above 0 health).
            // - We don't save if we're already below the threshold.
            // - We only save a maximum of once every MinTurnsBetweenSaves rounds
            int currentHP = E.Object.hitpoints;
            int maxHP = E.Object.baseHitpoints;

            if (E.Object.Health() < HealthThreshold)
                return false;

            double afterDamageHealth = ((double) (currentHP - E.Damage.Amount)) / ((double) maxHP);
            if (afterDamageHealth <= 0 || afterDamageHealth > HealthThreshold)
                return false;

            TriggerSave();

            Exit:
            return base.HandleEvent(E);
        }
    }
}