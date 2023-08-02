namespace Kernelmethod.CrungleMode {
    /// <summary>
    /// Thin layer over the mod's game options to determine what settings should be enabled or disabled
    /// when starting a new game.
    /// </summary>
    public class Options {
        public static bool EnableBasicSurvivalOptions => GetOption("Option_Kernelmethod_CrungleMode_BasicSurvival").EqualsNoCase("Yes");
        public static bool SpawnWithMakeCamp => EnableBasicSurvivalOptions || GetOption("Option_Kernelmethod_CrungleMode_SpawnWithMakeCamp").EqualsNoCase("Yes");
        public static bool SpawnWithSprint => EnableBasicSurvivalOptions || GetOption("Option_Kernelmethod_CrungleMode_SpawnWithSprint").EqualsNoCase("Yes");
        public static bool SpawnWithWater => EnableBasicSurvivalOptions || GetOption("Option_Kernelmethod_CrungleMode_SpawnWithWater").EqualsNoCase("Yes");
        public static bool SpawnWithLightSources => EnableBasicSurvivalOptions || GetOption("Option_Kernelmethod_CrungleMode_SpawnWithTorches").EqualsNoCase("Yes");

        public static bool NoAquatic => GetOption("Option_Kernelmethod_CrungleMode_NoAquatic").EqualsNoCase("Yes");
        public static bool NoLivesOnWalls => GetOption("Option_Kernelmethod_CrungleMode_NoLivesOnWalls").EqualsNoCase("Yes");
        public static bool SaveScores => GetOption("Option_Kernelmethod_CrungleMode_SaveScores").EqualsNoCase("Yes");

        private static string GetOption(string ID, string Default = "") => XRL.UI.Options.GetOption(ID, Default: Default);
    }
}