using XRL;
using XRL.UI;
using XRL.Wish;

namespace Kernelmethod.ChooseYourFighter {
    [HasWishCommand]
    public class WishHandler {
        [WishCommand(Command = "chooseyourfighter")]
        public static bool WishCommand(string rest) {
            switch (rest) {
            case "change":
                TileMenu.ChooseTileMenu(The.Player);
                break;
            default:
                Popup.ShowFail($"Unknown wish command for Kernelmethod.ChooseYourFighter: {rest}");
                break;
            }

            return true;
        }
    }
}
