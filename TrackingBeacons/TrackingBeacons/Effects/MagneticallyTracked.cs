using System;
using XRL.World;

namespace Kernelmethod.TrackingBeacons.Effects {
    [Serializable]
    public class MagneticallyTracked : Tracked {
        public MagneticallyTracked() : base(null) {}
        public MagneticallyTracked(GameObject Tracker = null) : base(Tracker) {}

        public override int GetEffectType() {
            return Effect.TYPE_FIELD | base.GetEffectType();
        }
    }
}
