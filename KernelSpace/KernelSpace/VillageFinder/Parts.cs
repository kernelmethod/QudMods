using Kernelmethod.KernelSpace;
using Qud.API;
using System;
using XRL.World;

namespace Kernelmethod.KernelSpace.Parts
{
    [Serializable]
    public class VillageFinder : IPart
    {
        public int VillageNavigationPercentageBonus {
            get => Options.VillageNavBonus ? 300 : 0;
        }

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
            E.NewBody?.RequirePart<VillageFinder>();
            E.OldBody?.RemovePart(typeof(VillageFinder));
            return base.HandleEvent(E);
        }
    }
}
