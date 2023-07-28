using XRL.World;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// ABC for classes that can filter out individual zones during zone sampling.
    /// </summary>
    public abstract class AbstractZoneFilter {
        public AbstractZoneFilter() {}

        public virtual bool IsValidZone(Zone Z) {
            return true;
        }
    }
}