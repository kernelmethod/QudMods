using XRL.World;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// ABC for classes that can filter out individual zones during zone sampling.
    /// </summary>
    public class VillageFilter : AbstractZoneFilter {
        public override bool IsValidZone(Zone Z) {
            return Z.HasBuilder("Village") || Z.HasBuilder("VillageOutskirts");
        }
    }
}