using Qud.API;
using System;
using System.Collections.Generic;
using XRL;
using XRL.Messages;
using XRL.World;

using Kernelmethod.TrackingBeacons;

namespace Kernelmethod.TrackingBeacons.Parts {
    [Serializable]
    public class TrackObject : IPart {
        // All of the map notes corresponding to objects being tracked
        // by this part.
        public HashSet<string> MapNoteIDs = new HashSet<string>();

        public GameObject Host = null;

        public void AddTrackingNote(string NoteID) {
            MapNoteIDs.Add(NoteID);
        }

        public void RemoveTrackingNote(string NoteID) {
            // Propagate note removals upwards
            if (Host != null && Host.TryGetPart<TrackObject>(out var part))
                part.RemoveTrackingNote(NoteID);

            MapNoteIDs.Remove(NoteID);

            if (MapNoteIDs.Count == 0)
                // Not tracking any objects anymore; safe to remove part.
                ParentObject.RemovePart(this);
        }

        public void TrackThroughHost(GameObject Host) {
            if (Options.DebugMessages)
                MessageQueue.AddPlayerMessage($"Tracking {ParentObject.DisplayName} through {Host.DisplayName}");
            UnregisterCurrentHost();

            this.Host = Host;
            var part = Host.RequirePart<TrackObject>();
            foreach (var noteID in MapNoteIDs) {
                part.AddTrackingNote(noteID);
            }
            part.UpdateMapNotes(Host.CurrentZone);
        }

        public void UnregisterCurrentHost() {
            if (Host != null && Host.TryGetPart<TrackObject>(out var part)) {
                if (Options.DebugMessages)
                    MessageQueue.AddPlayerMessage($"Unregistering {Host.DisplayName} as host for {ParentObject.DisplayName}");
                foreach (var note in MapNoteIDs)
                    part.RemoveTrackingNote(note);
            }

            Host = null;
        }

        public void UpdateMapNotes(Zone Z) {
            if (Options.DebugMessages)
                MessageQueue.AddPlayerMessage($"Running UpdateMapNotes for {ParentObject.DisplayName}; zone = {Z?.ZoneID}");
            if (Z is InteriorZone interiorZone) {
                Z = interiorZone.ResolveBasisZone() ?? Z;

                // If the interior zone is represented by some object in the
                // outside world, we will need to start tracking that object
                // rather than this one.
                if (interiorZone.ParentObject != null) {
                    TrackThroughHost(interiorZone.ParentObject);
                    return;
                }
            }

            var reveal = !(Z == null || Z.IsWorldMap() || Z.ZoneWorld != "JoppaWorld");
            var zoneID = Z?.ZoneID ?? "JoppaWorld";

            foreach (var noteID in MapNoteIDs) {
                var note = JournalAPI.GetMapNote(noteID);

                if (note == null) {
                    MetricsManager.LogInfo($"Tracking part could not find journal note with ID {noteID}");
                    RemoveTrackingNote(noteID);
                    continue;
                }

                note.ZoneID = zoneID;
                note.Revealed = reveal;
            }
        }

        public override bool WantEvent(int ID, int cascade) {
            return base.WantEvent(ID, cascade)
                || ID == EnteringZoneEvent.ID
                || ID == WasReplicatedEvent.ID
                || ID == EquippedEvent.ID
                || ID == UnequippedEvent.ID
                || ID == TakenEvent.ID
                || ID == DroppedEvent.ID;
        }

        public override bool HandleEvent(EnteringZoneEvent E) {
            UnregisterCurrentHost();
            UpdateMapNotes(E.Cell.ParentZone);

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(WasReplicatedEvent E) {
            // When an object is cloned, the clone shouldn't track all of the
            // beacons that are tracking the original object.
            if (ParentObject == E.Replica)
                ParentObject.RemovePart(this);

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(EquippedEvent E) {
            TrackThroughHost(E.Actor);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(UnequippedEvent E) {
            UnregisterCurrentHost();
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(TakenEvent E) {
            TrackThroughHost(E.Actor);
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(DroppedEvent E) {
            UnregisterCurrentHost();
            return base.HandleEvent(E);
        }
    }
}
