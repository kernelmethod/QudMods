using XRL;
using XRL.Wish;
using XRL.UI;

namespace Kernelmethod.SubmoduleManagement {
    [HasWishCommand]
    public class DebugWishHandler {
        [WishCommand(Command = "submodules")]
        public static bool WishCommand(string rest) {
            switch (rest) {
                case "list":
                    var submodules = "";
                    foreach (var file in DataManager.GetXMLFilesWithRoot("submodules")) {
                        submodules += file + "\n";
                    }
                    if (submodules.IsNullOrEmpty())
                        Popup.Show("No submodules found.");
                    else
                        Popup.Show("Found the following submodule files:\n\n"+ submodules);
                    break;
                default:
                    Popup.ShowFail($"Unknown wish command for Kernelmethod.SubmoduleManagement: {rest}");
                    break;
            }

            return true;
        }
    }
}
