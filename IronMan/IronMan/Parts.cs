using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XRL;
using XRL.Core;
using XRL.World;

namespace Kernelmethod.IronMan.Parts {
    [Serializable]
    public class IronManSavePart : IPart {
        public Dictionary<string, long> MinTurnsBetweenSaves = new Dictionary<string, long>() {
            { "HealthSaveThreshold", 300 },
            { "StatChange", 50 }
        };
        public Dictionary<string, long> LastSaveTurn = new Dictionary<string, long>();

        /// <summary>
        /// The threshold below which a player's health must drop before triggering a new save.
        /// </summary>
        public double HealthSaveThreshold = 0.4;

        public override bool AllowStaticRegistration() => true;

        public override void Register(GameObject obj) {
            obj.RegisterPartEvent(this, "StatChange_Strength");
            obj.RegisterPartEvent(this, "StatChange_Agility");
            obj.RegisterPartEvent(this, "statChange_Toughness");
            obj.RegisterPartEvent(this, "StatChange_Willpower");
            obj.RegisterPartEvent(this, "StatChange_Intelligence");
            obj.RegisterPartEvent(this, "StatChange_Ego");
            obj.RegisterPartEvent(this, "StatChange_Hitpoints");
            base.Register(obj);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == AfterPlayerBodyChangeEvent.ID
                || ID == AfterDieEvent.ID
                || ID == BeforeTookDamageEvent.ID;
        }

        public override bool HandleEvent(AfterPlayerBodyChangeEvent E) {
            E.NewBody?.RequirePart<IronManSavePart>();
            E.OldBody?.RemovePart(typeof(IronManSavePart));

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(AfterDieEvent E)
        {
            if (!ParentObject.IsPlayer())
                return base.HandleEvent(E);

            TriggerSave();
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(BeforeTookDamageEvent E)
        {
            if (!ParentObject.IsPlayer())
                return base.HandleEvent(E);

            // Check whether the damage we're about to take puts us below the threshold
            // (but above 0 health).
            // - We don't save if we're already below the threshold.
            // - We only save a maximum of once every MinTurnsBetweenSaves rounds
            int currentHP = E.Object.hitpoints;
            int maxHP = E.Object.baseHitpoints;

            if (E.Object.Health() < HealthSaveThreshold)
                return false;

            double afterDamageHealth = (currentHP - E.Damage.Amount) / maxHP;
            if (afterDamageHealth <= 0 || afterDamageHealth > HealthSaveThreshold)
                return false;

            TriggerSave("HealthSaveThreshold");

            return base.HandleEvent(E);
        }

        public override bool FireEvent(Event E) {
            if (!E.ID.StartsWith("StatChange_"))
                return base.FireEvent(E);

            // We only trigger a save on certain stat changes
            switch (E.ID) {
                case "StatChange_Strength":
                case "StatChange_Agility":
                case "StatChange_Toughness":
                case "StatChange_Willpower":
                case "StatChange_Intelligence":
                case "StatChange_Ego":
                case "StatChange_Hitpoints":
                    // Only save if the base value decreased
                    int oldBaseValue = E.GetIntParameter("OldBaseValue");
                    int newBaseValue = E.GetIntParameter("NewBaseValue");

                    if (newBaseValue >= oldBaseValue)
                        break;

                    TriggerSave("StatChange");
                    break;
                default:
                    // Do nothing
                    break;
            }

            return base.FireEvent(E);
        }

        public void TriggerSave(string key = null) {
            if (key != null) {
                var minTurns = MinTurnsBetweenSaves.GetValue(key, 0);
                var lastTurn = LastSaveTurn.GetValue(key, -1);

                if (minTurns > 0 && lastTurn > 0 && XRLCore.CurrentTurn - minTurns < lastTurn)
                    return;

                LastSaveTurn[key] = XRLCore.CurrentTurn;
            }

            The.Game.QuickSave();
        }
    }
}
