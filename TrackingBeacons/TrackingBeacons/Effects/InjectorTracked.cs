using System;
using XRL.World;

namespace Kernelmethod.TrackingBeacons.Effects {
    [Serializable]
    public class InjectorTracked : Tracked {

        public InjectorTracked() : base(null) {}
        public InjectorTracked(GameObject Tracker = null) : base(Tracker) {}

        public override int GetEffectType() {
            return Effect.TYPE_CIRCULATORY | base.GetEffectType();
        }
    }
}
