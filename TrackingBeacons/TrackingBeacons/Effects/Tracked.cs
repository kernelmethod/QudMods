using Qud.API;
using System;
using XRL;
using XRL.World;
using XRL.World.Effects;

using Kernelmethod.TrackingBeacons.Parts;

namespace Kernelmethod.TrackingBeacons.Effects {
    [Serializable]
    public abstract class Tracked : Effect {
        public GameObject Tracker = null;

        public string JournalNoteID;
        public JournalMapNote TrackerNote = null;

        public Tracked(GameObject Tracker = null) {
            this.Tracker = Tracker;
            DisplayName = "tracked";
            Duration = Effect.DURATION_INDEFINITE;
            JournalNoteID = Guid.NewGuid().ToString();
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

            JournalAPI.AddMapNote(
                "JoppaWorld",
                Object.DisplayName,
                "Tracking Beacons",
                secretId: JournalNoteID,
                revealed: false,
                sold: true,
                silent: true
            );
            var part = Object.RequirePart<TrackObject>();
            part.AddTrackingNote(JournalNoteID);
            part.UpdateMapNotes(Object.CurrentZone);
        }

        public override bool Apply(GameObject Object) {
            // Remove existing trackers from the object
            Object.RemoveEffect(typeof(Tracked));

            CreateTrackingNote(Object);
            return base.Apply(Object);
        }

        public void RemoveTracker(GameObject Object) {
            if (Object.TryGetPart<TrackObject>(out var part))
                part.RemoveTrackingNote(JournalNoteID);

            var note = JournalAPI.GetMapNote(JournalNoteID);
            if (note != null)
                JournalAPI.DeleteMapNote(note);
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
                || ID == WasReplicatedEvent.ID;
        }

        public override bool HandleEvent(BeforeDestroyObjectEvent E) {
            RemoveTracker(Object);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(WasReplicatedEvent E) {
            if (base.Object == E.Replica) {
                // Don't track the replica
                base.Object.RemoveEffect(this);
            }

            return base.HandleEvent(E);
        }
    }
}
