using System;
using XRL;
using XRL.Core;
using XRL.World;
namespace Kernelmethod.IronMan
{
    [Serializable]
    public class SaveOnDeath : IPart
    {
        public SaveOnDeath() {}

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == AfterDieEvent.ID;
        }

        public override bool HandleEvent(AfterDieEvent E)
        {
            The.Game.QuickSave();
            return base.HandleEvent(E);
        }
    }

    [Serializable]
    public class SaveOnHealthThreshold : IPart
    {
        public long LastSaveTurn;
        public long MinTurnsBetweenSaves;
        public double HealthThreshold;

        public SaveOnHealthThreshold()
        {
            MinTurnsBetweenSaves = 300;
            HealthThreshold = 0.4;

            // Initialize LastSaveTurn to -MinTurnBetweenSaves to ensure that we can save as early
            // as turn zero.
            LastSaveTurn = -MinTurnsBetweenSaves;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == BeforeTookDamageEvent.ID;
        }

        public override bool HandleEvent(BeforeTookDamageEvent E)
        {
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

            if (XRLCore.CurrentTurn - MinTurnsBetweenSaves < LastSaveTurn)
                return false;

            LastSaveTurn = XRLCore.CurrentTurn;
            The.Game.QuickSave();

            return base.HandleEvent(E);
        }
    }

    [Serializable]
    public class SaveOnDrainedStat : IPart
    {
        public long LastSaveTurn;
        public long MinTurnsBetweenSaves;

        public SaveOnDrainedStat()
        {
            MinTurnsBetweenSaves = 5;
            LastSaveTurn = -MinTurnsBetweenSaves;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == TookDamageEvent.ID;
        }

        public override bool HandleEvent(AttackerDealtDamageEvent E)
        {
            if (XRLCore.CurrentTurn - MinTurnsBetweenSaves < LastSaveTurn)
                return false;

            
            
            return base.HandleEvent(E);
        }

        public override bool FireEvent(Event E)
        {

            if (E.ID == "AttackerHit")
            {
                GameObject defender = E.GetGameObjectParameter("Defender");
                if (!defender.IsPlayer())
                    return false;

                if (!(E.GetStringParameter("Properties") ?? "").Contains("DrainedStat") && defender.IsPlayer())
                    return false;

                if (XRLCore.CurrentTurn - MinTurnsBetweenSaves < LastSaveTurn)
                    return false;

                LastSaveTurn = XRLCore.CurrentTurn;
                The.Game.QuickSave();
            }
            return base.FireEvent(E);
        }
    }

    // TODO: part for dismemberment
}