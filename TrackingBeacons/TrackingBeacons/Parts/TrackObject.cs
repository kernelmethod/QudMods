using HarmonyLib;
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
        // Global mapping between tracked GameObjects and their corresponding notes.
        public static Dictionary<string, string> TrackerNoteMapping = new Dictionary<string, string>();

        // Local set of objects being tracked through the current object.
        public HashSet<string> LocallyTracked = new HashSet<string>();

        public GameObject Host = null;

        public void StopTracking(string TrackerID) {
            // Propagate tracker removals upwards
            if (Host != null && Host.TryGetPart<TrackObject>(out var part))
                part.StopTracking(TrackerID);

            LocallyTracked.Remove(TrackerID);

            if (LocallyTracked.Count == 0)
                // Not tracking any objects anymore; safe to remove part.
                ParentObject.RemovePart(this);
        }

        public void TrackThroughHost(GameObject Host) {
            if (Options.DebugMessages)
                MessageQueue.AddPlayerMessage($"Tracking {ParentObject.DisplayName} through {Host.DisplayName}");

            UnregisterCurrentHost();

            this.Host = Host;
            var part = Host.RequirePart<TrackObject>();
            foreach (var tracked in LocallyTracked) {
                part.LocallyTracked.Add(tracked);
            }
            part.UpdateMapNotes(Host.CurrentZone);
        }

        public void UnregisterCurrentHost() {
            if (Host != null && Host.TryGetPart<TrackObject>(out var part)) {
                if (Options.DebugMessages)
                    MessageQueue.AddPlayerMessage($"Unregistering {Host.DisplayName} as host for {ParentObject.DisplayName}");
                foreach (var tracked in LocallyTracked)
                    part.StopTracking(tracked);
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

            foreach (var trackerID in LocallyTracked) {
                JournalMapNote note = null;
                if (TrackerNoteMapping.TryGetValue(trackerID, out var noteID)) {
                    note = JournalAPI.GetMapNote(noteID);
                    if (note != null)
                        JournalAPI.DeleteMapNote(note);
                }

                noteID = Guid.NewGuid().ToString();
                var text = note?.Text ?? "{{R|ERROR: unknown object}}";

                JournalAPI.AddMapNote(
                    zoneID,
                    text,
                    "Tracking Beacons",
                    secretId: noteID,
                    revealed: reveal,
                    sold: true,
                    silent: true
                );

                TrackerNoteMapping[trackerID] = noteID;
                Traverse.Create<XRL.UI.JournalScreen>().Field("LastHash").SetValue(0);
            }
        }

        public override bool WantEvent(int ID, int cascade) {
            return base.WantEvent(ID, cascade)
                || ID == EnteringZoneEvent.ID
                || ID == ReplicaCreatedEvent.ID
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

        public override bool HandleEvent(ReplicaCreatedEvent E) {
            // When an object is cloned, the clone shouldn't track all of the
            // beacons that are tracking the original object.
            if (ParentObject == E.Object)
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
