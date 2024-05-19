using System;
using XRL.World;

using Kernelmethod.TrackingBeacons.Effects;
using Kernelmethod.TrackingBeacons.Parts;

namespace XRL.World.Parts {
    [Serializable]
    public class Kernelmethod_TrackingBeacons_TrackerInjector : ITrackingApplicator {

        public override bool InvoluntaryApplicationRequiresPenetration {
            get => true;
        }

        public override void ApplyEffect(GameObject Target, GameObject Tracker)
            => Target.ApplyEffect(new InjectorTracked(Tracker));

        public override string ApplicationSFX {
            get => "Sounds/Interact/sfx_interact_applicator_apply";
        }

        public override bool CanApplyTo(GameObject gameObject) {
            return (gameObject.Physics?.Organic ?? false)
                && gameObject.GetIntProperty("Bleeds") > 0;
        }
    }
}
