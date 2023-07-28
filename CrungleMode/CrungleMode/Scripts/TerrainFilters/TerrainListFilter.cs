using System.Collections.Generic;
using XRL.World;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// A terrain filter that filters in all terrain that is contained within the ValidTerrain list.
    /// </summary>
    public class TerrainListFilter : AbstractTerrainFilter {
        public List<string> ValidTerrain = new List<string>();

        public override bool IsValidTerrain(GameObject terrain)
        {
            // MetricsManager.LogInfo("ValidTerrain = " + string.Join(", ", ValidTerrain));
            if (ValidTerrain.Count == 0)
                return base.IsValidTerrain(terrain);

            return ValidTerrain.Contains(terrain.Blueprint);
        }
    }
}