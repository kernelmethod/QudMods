using System;
using System.Collections.Generic;
using System.IO;
using XRL;
using XRL.Core;
using XRL.UI;
using XRL.World;

namespace Kernelmethod.IronMan.Parts {
    [Serializable]
    public class IronManSavePart : IPart {
        [NonSerialized]
        public long MinTurns = 50;
        [NonSerialized]
        public long PreviousSaveTurn = 0;

        [Obsolete("Field replaced with MinTurns.")]
        public Dictionary<string, long> MinTurnsBetweenSaves = null;
        [Obsolete("Field replaced with PreviousSaveTurn.")]
        public Dictionary<string, long> LastSaveTurn = null;

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

            int currentHP = E.Object.hitpoints;
            int maxHP = E.Object.baseHitpoints;

            if (E.Object.Health() < HealthSaveThreshold)
                return false;

            double afterDamageHealth = (currentHP - E.Damage.Amount) / maxHP;
            if (afterDamageHealth <= 0 || afterDamageHealth > HealthSaveThreshold)
                return false;

            TriggerSave();

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

                    TriggerSave();
                    break;
                default:
                    // Do nothing
                    break;
            }

            return base.FireEvent(E);
        }

        /// <summary>
        /// Create a new save for the current game, if sufficiently many turns have
        /// passed.
        /// </summary>
        public void TriggerSave() {
            if (MinTurns > 0 && PreviousSaveTurn > 0 && XRLCore.CurrentTurn - MinTurns < PreviousSaveTurn)
                return;

            PreviousSaveTurn = XRLCore.CurrentTurn;
            The.Game.QuickSave();
        }

        /// <summary>
        /// Delete the save for the current game.
        /// </summary>
        public void TriggerDelete() {
            if (!Options.DisablePermadeath) {
                var cacheDirectory = The.Game.GetCacheDirectory();
                if (cacheDirectory != null)
                    Directory.Delete(cacheDirectory, recursive: true);
            }
        }
    }
}
