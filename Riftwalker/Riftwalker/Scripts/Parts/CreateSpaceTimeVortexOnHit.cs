using System;

namespace XRL.World.Parts {
    /// <summary>
    /// Part that can be applied to projectiles to make them create a space-time vortex on hit.
    /// </summary>
    [Serializable]
    public class Kernelmethod_Riftwalker_CreateSpaceTimeVortexOnHit : IActivePart
    {
        public Kernelmethod_Riftwalker_CreateSpaceTimeVortexOnHit ()
        {
            IsRealityDistortionBased = true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "ProjectileHit");           
        }
  
        public override bool FireEvent(Event E)
        {
            if (E.ID == "ProjectileHit")
            {
                GameObject Attacker = E.GetParameter("Attacker") as GameObject;
                GameObject Defender = E.GetParameter("Defender") as GameObject;
                if (Defender == null)
                {
                    return true;
                }

                // Make defender hostile towards the attacker
                Defender.GetAngryAt(Attacker, -5);

                // Create a new vortex at the location of the defender
                Cell cell = Defender.CurrentCell;
                if (cell == null)
                {
                    return true;
                }

                GameObject vortex = GameObject.create("Space-Time Vortex");
                cell.AddObject(vortex);
            }

            return base.FireEvent(E);
        }
    }
}