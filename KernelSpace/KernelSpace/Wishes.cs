using System.IO;
using XRL;
using XRL.UI;
using XRL.Wish;

namespace Kernelmethod.KernelSpace {
    [HasWishCommand]
    public class KernelSpaceWishHandler {
        [WishCommand(Command = "exportmessages")]
        public static bool ExportMessagesWishHandler() {
            var cacheDirectory = The.Game.GetCacheDirectory();
            var messagesPath = Path.Combine(cacheDirectory, "Messages.txt");

            using (var outputFile = new StreamWriter(messagesPath)) {
                foreach (var message in The.Game.Player.Messages.Messages) {
                    outputFile.WriteLine(message);
                }
            }

            Popup.Show($"Wrote messages to {messagesPath}", LogMessage: false);
            return true;
        }
    }
}
