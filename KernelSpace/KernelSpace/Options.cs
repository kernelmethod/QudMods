namespace Kernelmethod.KernelSpace {
    public class Options {
        public static bool VillageNavBonus =>
            XRL.UI.Options.GetOption("Option_KernelSpace_VillageNavBonus").EqualsNoCase("Yes");

        public static bool DisableEatingSounds =>
            XRL.UI.Options.GetOption("Option_KernelSpace_DisableEatingSounds").EqualsNoCase("Yes");
    }
}
