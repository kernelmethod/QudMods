using XRL.UI;
using XRL.Wish;

namespace Kernelmethod.ChooseYourFighter {
    [HasWishCommand]
    public class WishHandler {
        [WishCommand(Command = "chooseyourfighter")]
        public static bool WishCommand(string rest) {
            switch (rest) {
            case "change":
                var model = TileMenu.ChooseTileMenu();
                if (model != null)
                    TileFactory.ChangePlayerAppearance(model);
                break;
            default:
                Popup.ShowFail($"Unknown wish command for Kernelmethod.ChooseYourFighter: {rest}");
                break;
            }

            return true;
        }
    }
}
