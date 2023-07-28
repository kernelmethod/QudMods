namespace Kernelmethod.CrungleMode {
    /// <summary>
    /// Thin layer over the mod's game options to determine what settings should be enabled or disabled
    /// when starting a new game.
    /// </summary>
    public class Options {
        public static bool EnableBasicSurvivalOptions {
            get {
                return GetOption("Option_Kernelmethod_CrungleMode_BasicSurvival") == "Yes";
            }
        }

        public static bool SpawnWithMakeCamp {
            get {
                return EnableBasicSurvivalOptions || GetOption("Option_Kernelmethod_CrungleMode_SpawnWithMakeCamp") == "Yes";
            }
        }

        public static bool SpawnWithSprint {
            get {
                return EnableBasicSurvivalOptions || GetOption("Option_Kernelmethod_CrungleMode_SpawnWithSprint") == "Yes";
            }
        }

        public static bool SpawnWithWater {
            get {
                return EnableBasicSurvivalOptions || GetOption("Option_Kernelmethod_CrungleMode_SpawnWithWater") == "Yes";
            }
        }

        public static bool SpawnWithLightSources {
            get {
                return EnableBasicSurvivalOptions || GetOption("Option_Kernelmethod_CrungleMode_SpawnWithTorches") == "Yes";
            }
        }

        public static bool NoAquatic {
            get {
                return GetOption("Option_Kernelmethod_CrungleMode_NoAquatic") == "Yes";
            }
        }

        public static bool NoLivesOnWalls {
            get {
                return GetOption("Option_Kernelmethod_CrungleMode_NoLivesOnWalls") == "Yes";
            }
        }

        public static bool SaveScores {
            get {
                return GetOption("Option_Kernelmethod_CrungleMode_SaveScores") == "Yes";
            }
        }

        private static string GetOption(string ID, string Default = "") {
            return XRL.UI.Options.GetOption(ID, Default=Default);
        }
    }
}