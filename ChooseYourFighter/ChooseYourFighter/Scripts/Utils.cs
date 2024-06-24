using System.Text;

namespace Kernelmethod.ChooseYourFighter {
    public static class Utils {
        public static void LogInfo(string Message, string Context = null) {
            var builder = new StringBuilder();
            builder.Append("Kernelmethod_ChooseYourFighter");

            if (Context != null)
                builder.Append("::")
                    .Append(Context);

            builder.Append(":")
                .Append(Message);
            MetricsManager.LogInfo(builder.ToString());
        }
    }
}
