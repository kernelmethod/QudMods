using XRL.World;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// Custom TerrainFilter for Palladium Reef terrain.
    /// </summary>
    public class PalladiumReefFilter : AbstractTerrainFilter {
        public override bool IsValidTerrain(GameObject terrain)
        {
            return terrain.Blueprint.Contains("TerrainPalladiumReef");
        }
    }
}