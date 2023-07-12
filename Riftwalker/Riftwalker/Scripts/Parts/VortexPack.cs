using Kernelmethod.Riftwalker.Utilities;

using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts
{
    /// <summary>
    /// A part that can be applied to items to grant the ability to spawn a space-time
    /// vortex nearby.
    /// </summary>
    [Serializable]
    public class Kernelmethod_Riftwalker_VortexPack : IPoweredPart
    {
        public Guid ActivatedAbilityID = Guid.Empty;
        public int CooldownRemaining;

        public static string PartEventName = "ActivateVortexPack";
        private static string Blueprint = "Kernelmethod_Riftwalker_Dissipating Space-Time Rift";

        public Kernelmethod_Riftwalker_VortexPack()
        {
            WorksOnSelf = true;
            ChargeUse = 4000;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (ID == EquippedEvent.ID)
                return true;
            if (ID == UnequippedEvent.ID)
                return true;

            return base.WantEvent(ID, cascade);
        }

        public override bool HandleEvent(EquippedEvent E)
        {
            if (!ParentObject.IsEquippedProperly())
                goto End;

            E.Actor.RegisterPartEvent(this, PartEventName);
            ActivatedAbilityID = E.Actor.AddActivatedAbility("Activate Vortex Pack", PartEventName, "Items");

            if (CooldownRemaining != 0)
            {
                ActivatedAbilityEntry ability = E.Actor.GetActivatedAbility(ActivatedAbilityID);
                if (ability != null)
                    ability.Cooldown = CooldownRemaining;
                CooldownRemaining = 0;
            }

        End:
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(UnequippedEvent E)
        {
            E.Actor.UnregisterPartEvent(this, PartEventName);

            ActivatedAbilityEntry ability = E.Actor.GetActivatedAbility(ActivatedAbilityID);
            if (ability != null)
                CooldownRemaining = ability.Cooldown;

            E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
            return base.HandleEvent(E);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID != PartEventName)
                goto End;

            GameObject equipped = ParentObject.Equipped;
            if (equipped == null || !equipped.IsPlayer())
                goto End;

            Cell cell = equipped.pPhysics.PickDirection("Create Rift");
            if (cell == null)
                goto End;

            if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false,
                IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false,
                MultipleCharge: 1, ChargeUse: null, UseChargeIfUnpowered: false, GridMask: 0L))
            {
                Popup.Show(ParentObject.T() + ParentObject.Is + " unresponsive.");
                goto End;
            }

            CreateVortex(cell);
            equipped.CooldownActivatedAbility(ActivatedAbilityID, 20);

        End:
            return base.FireEvent(E);
        }

        /// <summary>
        /// Create a space-time vortex at the chosen location.
        /// </summary>
        private void CreateVortex(Cell cell)
        {
            GameObject vortex = GameObject.create(Blueprint);

            Temporary temporary = vortex.GetPart("Temporary") as Temporary;
            int duration = Kernelmethod_Riftwalker_Random.Next(40, 60);
            temporary.Duration = duration;

            vortex.ForceApplyEffect(new QuantumStabilized(duration));
            cell.AddObject(vortex);
        }
    }
}