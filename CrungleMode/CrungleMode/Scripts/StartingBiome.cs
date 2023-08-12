using System;
using System.Collections.Generic;
using System.Linq;
using XRL;
using XRL.CharacterBuilds.Qud;
using XRL.World;

namespace Kernelmethod.CrungleMode {
    public class StartingBiome {
        private static void LogInfo(string message) {
            MetricsManager.LogInfo($"Kernelmethod_CrungleMode: {message}");
        }

        public static bool IsBiomeTerrain(Kernelmethod_CrungleMode_BiomeGameModule.BiomeData biome, GameObject terrain) {
            return biome.TerrainFilters.Select(f => f.IsValidTerrain(terrain)).Contains(true);
        }

        /// <summary>
        /// Return an ID for a random zone of the given type.
        /// </summary>
        public static IEnumerable<string> GetRandomDestinationZoneIDs(Kernelmethod_CrungleMode_BiomeGameModule.BiomeData biome, int num)
        {
            int parasangX, parasangY;
            LogInfo($"sampling random destination zone for BiomeType {biome}");

            // Create a list of all of the zones with the given biome
            var zones = new BallBag<(int, int)>();
            for (parasangX = 0; parasangX < 80; parasangX++) {
                for (parasangY = 0; parasangY < 25; parasangY++) {
                    var id = ZoneID.Assemble("JoppaWorld", parasangX, parasangY, 1, 1, 10);
                    var terrain = The.ZoneManager.GetZoneTerrain("JoppaWorld", parasangX, parasangY);

                    if (IsBiomeTerrain(biome, terrain))
                        zones.Add((parasangX, parasangY), 100);
                }
            }

            LogInfo($"found {zones.Count} matches for BiomeType {biome}");
            if (zones.Count == 0)
                throw new Exception("No zones found for BiomeType " + biome);

            for (int i = 0; i < num; i++) {
                // Pick a random parasang satisfying the terrain conditions
                (parasangX, parasangY) = zones.PeekOne();

                // Pick a random location within that parasang
                (var zoneX, var zoneY, var zoneZ) = biome.ZoneXYZ.ChooseXYZ("JoppaWorld", parasangX, parasangY);
                var zone = ZoneID.Assemble("JoppaWorld", parasangX, parasangY, zoneX, zoneY, zoneZ);
                LogInfo($"sampled random zone {zone}");
                yield return zone;
            }
        }
    }
}
