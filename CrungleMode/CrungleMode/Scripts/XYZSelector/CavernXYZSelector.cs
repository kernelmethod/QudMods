using Kernelmethod.CrungleMode.Utilities;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// Zone X/Y/Z selector for the cavern biome.
    /// </summary>
    public class CavernXYZSelector : XYZSelector {
        public override int ChooseZ(string world, int parasangX, int parasangY)
        {
            return Kernelmethod_CrungleMode_Random.Next(5, 30);
        }
    }
}