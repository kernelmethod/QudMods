using HarmonyLib;
using Qud.API;
using System.Collections.Generic;
using System.Linq;

namespace Kernelmethod.TrackingBeacons.Patches {
    /// <summary>
    /// Don't reveal tracking beacon notes when calling JournalAPI.GetMapNotesForZone.
    ///
    /// This will suppress tracking beacon notes from being added to the message log
    /// every time the player enters a zone.
    /// </summary>
    [HarmonyPatch(typeof(JournalAPI), nameof(JournalAPI.GetMapNotesForZone))]
    public static class JournalAPIPatches {
        public static void Postfix(ref List<JournalMapNote> __result) {
            // Filter out notes that correspond to tracking beacons
            __result = __result.Where(note => note.Category != "Tracking Beacons")
                .ToList();
        }
    }
}
