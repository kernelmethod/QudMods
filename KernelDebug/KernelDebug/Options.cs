namespace Kernelmethod.KernelDebug {
    public class Options {
        public static bool ShowTemperature =>
            XRL.UI.Options.GetOption("Option_KernelDebug_ShowTemperature").EqualsNoCase("Yes");
    }
}
