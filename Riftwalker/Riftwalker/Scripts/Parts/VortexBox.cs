using Kernelmethod.Riftwalker.Utilities;

using System;

namespace XRL.World.Parts
{
    /// <summary>
    /// Part granting the vortex-in-a-box's ability to create space-time vortices.
    /// </summary>
    [Serializable]
    public class Kernelmethod_Riftwalker_VortexBox : IGrenade
    {
        public static string PartEventName = "Detonate";

        public static string Blueprint = "Space-Time Vortex";

        public override bool WantEvent(int ID, int cascade)
        {
            if (ID == GetInventoryActionsEvent.ID)
                return true;
            if (ID == InventoryActionEvent.ID)
                return true;

            return base.WantEvent(ID, cascade);
        }

        public override bool HandleEvent(GetInventoryActionsEvent E)
        {
            E.AddAction("Open", "open", PartEventName, null, 'o', FireOnActor: false, Default: 0,
                Priority: 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);

            // Don't propagate handling up to IGrenade, so that 'detonate' isn't added to the
            // list of actions.
            return true;
        }

        protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
        {
            DidX("shatter", null, "!");
            CreateVortex(C);
            return true;
        }

        /// <summary>
        /// Create a dissipating vortex at the provided location..
        /// </summary>
        private void CreateVortex(Cell cell)
        {
            GameObject vortex = GameObject.create(Blueprint);
            Temporary temporary = vortex.GetPart("Temporary") as Temporary;
            int duration = Kernelmethod_Riftwalker_Random.Next(10, 18);
            temporary.Duration = duration;

            // Destroy the box before creating the vortex so that it doesn't get sucked in
            ParentObject.Destroy(null, Silent: true);

            // vortex.ForceApplyEffect(new QuantumStabilized(duration));
            cell.AddObject(vortex);
        }
    }
}