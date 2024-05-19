using Qud.API;
using System;
using System.Linq;
using XRL;
using XRL.World;
using XRL.World.Effects;

using Kernelmethod.TrackingBeacons.Parts;

namespace Kernelmethod.TrackingBeacons.Effects {
    [Serializable]
    public abstract class Tracked : Effect {
        public GameObject Tracker = null;

        public string TrackerID;
        public JournalMapNote TrackerNote = null;

        public Tracked(GameObject Tracker = null) {
            this.Tracker = Tracker;
            DisplayName = "tracked";
            Duration = Effect.DURATION_INDEFINITE;
            TrackerID = Guid.NewGuid().ToString();
        }

        public override string GetDetails() {
            return "Location tracked in the journal";
        }

        public override int GetEffectType() {
            return Effect.TYPE_MINOR | Effect.TYPE_REMOVABLE;
        }

        public void CreateTrackingNote(GameObject Object) {
            if (Tracker == null || !Tracker.IsPlayer())
                // Not being tracked by player; do nothing.
                return;

            var noteID = Guid.NewGuid().ToString();
            JournalAPI.AddMapNote(
                "JoppaWorld",
                Object.DisplayName,
                "Tracking Beacons",
                secretId: noteID,
                revealed: false,
                sold: true,
                silent: true
            );
            TrackObject.TrackerNoteMapping.Add(TrackerID, noteID);

            var part = Object.RequirePart<TrackObject>();
            part.LocallyTracked.Add(TrackerID);
            part.UpdateMapNotes(Object.CurrentZone);
        }

        public override bool Apply(GameObject Object) {
            // Remove existing trackers from the object
            var trackingEffects = Object.Effects
                .Where(e => e is Tracked)
                .ToList();

            foreach (var effect in trackingEffects)
                Object.RemoveEffect(effect);

            CreateTrackingNote(Object);
            return base.Apply(Object);
        }

        public void RemoveTracker(GameObject Object) {
            if (Object.TryGetPart<TrackObject>(out var part))
                part.StopTracking(TrackerID);

            if (TrackerID == null)
                return;

            if (TrackObject.TrackerNoteMapping.TryGetValue(TrackerID, out var noteID)) {
                var note = JournalAPI.GetMapNote(noteID);
                if (note != null)
                    JournalAPI.DeleteMapNote(note);
            }

            TrackObject.TrackerNoteMapping.Remove(TrackerID);
        }

        public override void Remove(GameObject Object) {
            RemoveTracker(Object);
            base.Remove(Object);
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar) {
            Registrar.Register("ApplyEMP");
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

        public override bool WantEvent(int ID, int cascade) {
            return base.WantEvent(ID, cascade)
                || ID == BeforeDestroyObjectEvent.ID
                || ID == ReplicaCreatedEvent.ID;
        }

        public override bool HandleEvent(BeforeDestroyObjectEvent E) {
            RemoveTracker(Object);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(ReplicaCreatedEvent E) {
            // Don't track the replica
            if (base.Object == E.Object) {
                // Set tracker ID to null so that removing the effect doesn't delete
                // the map note for the original object.
                TrackerID = null;
                base.Object.RemoveEffect(this);
            }

            return base.HandleEvent(E);
        }
    }
}
