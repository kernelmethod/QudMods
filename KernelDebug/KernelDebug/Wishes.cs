using XRL;
using XRL.UI;
using XRL.Wish;
using XRL.World;

namespace Kernelmethod.KernelDebug {
    [HasWishCommand]
    public class KernelDebugWishHandler {
        [WishCommand(Command = "settemperature")]
        public static void SetTemperatureWish() {
            var gameObject = The.Player
                .Physics
                .PickDestinationCell(
                    9999, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Set temperature for which object?"
                )?
                .GetHighestRenderLayerObject();

            if (gameObject == null || gameObject.Physics == null)
                return;

            var input = Popup.AskNumber("What should the temperature be set to?", 0, -9999999, 9999999);
            if (input == null)
                return;

            gameObject.Physics.Temperature = input ?? 25;
        }
    }
}
