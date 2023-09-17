using Qud.API;
using System;
using XRL.World;

namespace Kernelmethod.VillageFinder
{
    [Serializable]
    public class EncounterModifier : IPart
    {
        public int VillageNavigationPercentageBonus = 300;

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == EncounterChanceEvent.ID
                || ID == AfterPlayerBodyChangeEvent.ID;
        }

        public override bool HandleEvent(EncounterChanceEvent E) {
            var secretID = E.Encounter?.secretID;
            var journalEntry = string.IsNullOrEmpty(secretID) ? null : JournalAPI.GetMapNote(secretID);

            if (E.Encounter == null || journalEntry == null || !journalEntry.Has("villages"))
                return base.HandleEvent(E);

            E.PercentageBonus += VillageNavigationPercentageBonus;
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(AfterPlayerBodyChangeEvent E) {
            E.NewBody?.RequirePart<EncounterModifier>();
            E.OldBody?.RemovePart(typeof(EncounterModifier));
            return base.HandleEvent(E);
        }
    }
}
