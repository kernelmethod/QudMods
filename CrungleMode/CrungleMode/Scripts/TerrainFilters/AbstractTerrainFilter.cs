using XRL.World;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// Interface for classes that can be used to dynamically filter in or filter out terrain
    /// that can be used to select an embark zone for Crungle Mode.
    /// </summary>
    public abstract class AbstractTerrainFilter {
        public AbstractTerrainFilter() {}

        /// <summary>
        /// Return True if a terrain GameObject can be used as the embark zone.
        /// </summary>
        public virtual bool IsValidTerrain(GameObject terrain) {
            return true;
        }
    }
}