namespace Kernelmethod.TrackingBeacons {
    public static class Options {
        public static bool DebugMessages =>
            XRL.UI.Options.GetOption("Option_TrackingBeacons_DebugMessages").EqualsNoCase("Yes");
    }
}
