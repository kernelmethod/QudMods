using HarmonyLib;
using Qud.API;
using System;
using System.Linq;
using XRL;
using XRL.Messages;
using XRL.World;
using XRL.World.Effects;

using Kernelmethod.TrackingBeacons;

namespace Kernelmethod.TrackingBeacons.Effects {
    [Serializable]
    public abstract class Tracked : IScribedEffect {
        public GameObject Tracker = null;

        public JournalMapNote TrackerNote = null;

        public string NoteID = null;

        // The object that is actually being tracked in the journal, _not_ the object
        // that the effect is applied to. These can be different in certain cases
        // (e.g., the tracker was applied to an object that is not in another creature's
        // inventory).
        public GameObject TrackedObject = null;

        private XRL.Version? MigrateFrom = null;

        public Tracked(GameObject Tracker = null) {
            this.Tracker = Tracker;
            DisplayName = "tracked";
            Duration = Effect.DURATION_INDEFINITE;
        }

        public override string GetDetails() {
            return "Location tracked in the journal";
        }

        public override int GetEffectType() {
            return Effect.TYPE_MINOR | Effect.TYPE_REMOVABLE;
        }

        public override bool Apply(GameObject Object) {
            // Remove existing trackers from the object
            var trackingEffects = Object.Effects
                .Where(e => e is Tracked)
                .ToList();

            foreach (var effect in trackingEffects)
                Object.RemoveEffect(effect);

            UpdateMapNotes(Object.CurrentZone);
            return base.Apply(Object);
        }

        public void RemoveTracker(GameObject Object) {
            var note = JournalAPI.GetMapNote(NoteID);
            if (note != null)
                JournalAPI.DeleteMapNote(note);
        }

        public void UpdateMapNotes(Zone Z) {
            if (Options.DebugMessages)
                MessageQueue.AddPlayerMessage($"Running UpdateMapNotes for {Object.DisplayName}; zone = {Z?.ZoneID}");
            if (Z is InteriorZone interiorZone) {
                Z = interiorZone.ResolveBasisZone() ?? Z;

                // If the interior zone is represented by some object in the
                // outside world, we will need to start tracking that object
                // rather than this one.
                if (interiorZone.ParentObject != null) {
                    TrackedObject = interiorZone.ParentObject;
                    ApplyRegistrar(Object);
                    return;
                }
            }

            var reveal = !(Z == null || Z.IsWorldMap() || Z.ZoneWorld != "JoppaWorld");
            var zoneID = Z?.ZoneID ?? "JoppaWorld";

            var note = JournalAPI.GetMapNote(NoteID);
            if (note != null)
                JournalAPI.DeleteMapNote(note);

            NoteID = Guid.NewGuid().ToString();
            JournalAPI.AddMapNote(
                zoneID,
                Object.DisplayName,
                "Tracking Beacons",
                secretId: NoteID,
                revealed: reveal,
                sold: true,
                silent: true
            );

            Traverse.Create<XRL.UI.JournalScreen>().Field("LastHash").SetValue(0);
        }

        public override void Remove(GameObject Object) {
            RemoveTracker(Object);
            base.Remove(Object);
        }

        public override bool WantEvent(int ID, int cascade) {
            return base.WantEvent(ID, cascade)
                || ID == BeforeDestroyObjectEvent.ID
                || ID == ReplicaCreatedEvent.ID
                || ID == EquippedEvent.ID
                || ID == UnequippedEvent.ID
                || ID == TakenEvent.ID
                || ID == DroppedEvent.ID
                || ID == AfterGameLoadedEvent.ID;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar) {
            var enteringZoneObject = (TrackedObject != null && GameObject.Validate(ref TrackedObject))
                ? TrackedObject
                : Object;

            Registrar.Register("ApplyEMP");
            Registrar.Register(enteringZoneObject, EnteringZoneEvent.ID);
            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E) {
            if (E.ID == "ApplyEMP") {
                IComponent<GameObject>.AddPlayerMessage(
                    Object.poss("beacon") + " is destroyed by the EMP.",
                    IComponent<GameObject>.ConsequentialColor(
                        ColorAsBadFor: Tracker
                    )
                );

                Object.RemoveEffect(this);
            }

            return base.FireEvent(E);
        }

        public override bool HandleEvent(BeforeDestroyObjectEvent E) {
            RemoveTracker(Object);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(ReplicaCreatedEvent E) {
            // Don't track the replica
            if (base.Object == E.Object) {
                base.Object.RemoveEffect(this);
            }

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(EnteringZoneEvent E) {
            UpdateMapNotes(E.Cell.ParentZone);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(EquippedEvent E) {
            TrackedObject = E.Actor;
            ApplyRegistrar(Object);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(UnequippedEvent E) {
            ApplyRegistrar(Object);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(TakenEvent E) {
            TrackedObject = E.Actor;
            ApplyRegistrar(Object);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(DroppedEvent E) {
            ApplyRegistrar(Object);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(AfterGameLoadedEvent E) {
            // Migrate from older versions of the mod.
            if (MigrateFrom == null)
                return base.HandleEvent(E);

            // Remove the effect altogether.
            MetricsManager.LogInfo($"TrackingBeacons::Tracked: migrating effect from {MigrateFrom}");
            Object.RemoveEffect(this);

            foreach (var note in JournalAPI.MapNotes) {
                if (note.Category == "Tracking Beacons")
                    JournalAPI.DeleteMapNote(note);
            }

            return true;
        }

        public override void Read(GameObject Basis, SerializationReader Reader) {
            // Migrate from older versions of the mod
            var modVersion = Reader.ModVersions["Kernelmethod_TrackingBeacons"];

            if (modVersion >= (new XRL.Version("0.2.0"))) {
                base.Read(Basis, Reader);
                return;
            }

            MigrateFrom = modVersion;

            // Version < 0.2.0
            // Read off old fields so that deserialization can continue
            MigrateFrom = modVersion;
            Reader.ReadGameObject();
            Reader.ReadString();
            Reader.ReadObject();
        }
    }
}
