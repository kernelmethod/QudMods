namespace XRL.UI {
    [HasModSensitiveStaticCache]
    public class Kernelmethod_Riftwalker_Options
    {
        public static bool EnableItems => GetOption("Option_Kernelmethod_Riftwalker_Items").EqualsNoCase("Yes");

        public static string GetOption(string ID, string Default = "")
        {
            return Options.GetOption(ID, Default);
        }
    }
}