using System;
using XRL.World;
using XRL.World.Parts;

using Kernelmethod.TrackingBeacons.Effects;
using Kernelmethod.TrackingBeacons.Parts;

namespace XRL.World.Parts {
    [Serializable]
    public class Kernelmethod_TrackingBeacons_MagneticBeacon : ITrackingApplicator {
        public override void ApplyEffect(GameObject Target, GameObject Tracker)
            => Target.ApplyEffect(new MagneticallyTracked(Tracker));

        public override string ApplicationSFX {
            get => "Sounds/Abilities/sfx_ability_magnetic_pulse";
        }

        public override bool CanApplyTo(GameObject gameObject) {
            return gameObject.HasPart<Metal>();
        }
    }
}
