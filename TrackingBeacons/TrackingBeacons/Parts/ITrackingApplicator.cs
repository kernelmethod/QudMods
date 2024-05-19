using System;
using System.Linq;
using System.Text;
using XRL;
using XRL.Language;
using XRL.UI;
using XRL.World;

using Kernelmethod.TrackingBeacons.Effects;

namespace Kernelmethod.TrackingBeacons.Parts {
    [Serializable]
    public abstract class ITrackingApplicator : IPart {

        public virtual bool InvoluntaryApplicationRequiresPenetration {
            get => false;
        }

        public abstract void ApplyEffect(GameObject Target, GameObject Tracker);
        public abstract string ApplicationSFX { get; }

        /// <summary>
        /// Return true if the tracking beacon applicator can be applied to the
        /// specified object, and false otherwise.
        /// </summary>
        public abstract bool CanApplyTo(GameObject gameObject);

        public override bool WantEvent(int ID, int cascade) {
            return base.WantEvent(ID, cascade)
                || ID == InventoryActionEvent.ID;
        }

        public override bool HandleEvent(InventoryActionEvent E) {
            if (E.Command == "Apply") {
                MetricsManager.LogInfo("running Apply handler");
                if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
                    return false;

                if (E.Item.IsBroken()) {
                    E.Actor.Fail(E.Item.Itis + " broken...");
                    return false;
                }
                if (E.Item.IsRusted()) {
                    E.Actor.Fail(E.Item.Itis + " rusted...");
                    return false;
                }
                if (E.Item.IsEMPed()) {
                    E.Actor.Fail(E.Item.Itis + " disabled...");
                    return false;
                }

                MetricsManager.LogInfo("picking direction");
                var cell = E.Actor.Physics.PickDirection("Apply Beacon");
                if (cell == null)
                    return false;

                // Identify all of the organic objects that bleed in the cell
                var organicObjects = cell
                    .GetObjectsInCell()
                    .Where(o => CanApplyTo(o))
                    .ToList();

                if (organicObjects.Count == 0) {
                    var builder = new StringBuilder();
                    return E.Actor.Fail("There is nothing there that you can apply " + ParentObject.t(Single: true) + " to.");
                }

                GameObject target = null;
                if (organicObjects.Count == 1)
                    target = organicObjects[0];
                else if (E.Actor.IsPlayer()) {
                    var options = organicObjects
                        .Select((GameObject o) => o.DisplayName)
                        .ToArray();

                    var icons = organicObjects
                        .Select((GameObject o) => o.RenderForUI())
                        .ToArray();

                    // Prompt the player to choose which object to inject
                    var selection = Popup.ShowOptionList(
                        "Select an object",
                        options,
                        Icons: icons,
                        AllowEscape: true
                    );

                    if (selection == -1)
                        return false;

                    target = organicObjects[selection];
                }

                if (target == null)
                    return false;

                if (
                    target.IsHostileTowards(E.Actor)
                    || (
                        !target.IsLedBy(E.Actor)
                        && target != E.Actor
                        && GetUtilityScoreEvent.GetFor(target, ParentObject, ForPermission: true) <= 0
                        && target.Brain != null
                       )
                ) {
                    var builder = new StringBuilder();
                    builder.Append(target.Does("do"))
                        .Append(" not want ")
                        .Append(ParentObject.t(Single: true))
                        .Append(" applied to ")
                        .Append(target.itself)
                        .Append(". You'll need to equip ")
                        .Append(ParentObject.them)
                        .Append(" as a weapon and attack with ")
                        .Append(ParentObject.them)
                        .Append(".");

                    return E.Actor.Fail(builder.ToString());
                }

                ApplyBeacon(target, E.Actor);
            }

            return base.HandleEvent(E);
        }

        public void ApplyBeacon(GameObject Target, GameObject Tracker) {
            if (!CanApplyTo(Target) || IsBroken() || IsRusted() || IsEMPed()) {
                var builder = new StringBuilder();
                builder.Append(ParentObject.Does("bounce"))
                    .Append(" off of ")
                    .Append(Target.t())
                    .Append(" and")
                    .Append(ParentObject.Is)
                    .Append(" destroyed.");

                IComponent<GameObject>.AddPlayerMessage(
                    builder.ToString(),
                    IComponent<GameObject>.ConsequentialColor(ColorAsBadFor: Tracker)
                );

                ParentObject.Destroy(Silent: true);
                return;
            }

            ParentObject.SplitFromStack();
            IComponent<GameObject>.WDidXToYWithZ(Tracker, "apply", ParentObject, "to", Target, ColorAsGoodFor: Tracker, ColorAsBadFor: Target);
            Tracker.PlayWorldOrUISound(ApplicationSFX);

            ApplyEffect(Target, Tracker);
            ParentObject.Destroy(Silent: true);
        }

        public override void Register(GameObject gameObject, IEventRegistrar Registrar) {
            Registrar.Register("ProjectileHit");
            Registrar.Register("ThrownProjectileHit");
            Registrar.Register("WeaponAfterDamage");
            base.Register(gameObject, Registrar);
        }

        public override bool FireEvent(Event E) {
            if (
                E.ID == "ProjectileHit"
                || E.ID == "ThrownProjectileHit"
                || E.ID == "WeaponAfterDamage"
            )
            {
                var target = E.GetGameObjectParameter("Defender");
                if (!GameObject.Validate(ref target))
                    return base.FireEvent(E);

                if (
                    InvoluntaryApplicationRequiresPenetration
                    && E.GetIntParameter("Penetrations") <= 0
                )
                {
                    var builder = new StringBuilder();
                    builder.Append(ParentObject.Does("fail"))
                        .Append(" to penetrate ")
                        .Append(target.poss("armor"))
                        .Append(" and")
                        .Append(ParentObject.Is)
                        .Append(" destroyed.");

                    IComponent<GameObject>.AddPlayerMessage(builder.ToString());
                    ParentObject.Destroy(Silent: true);
                }
                else {
                    var tracker = E.GetGameObjectParameter("Attacker");
                    ApplyBeacon(target, tracker);
                }
            }

            return base.FireEvent(E);
        }
    }
}
