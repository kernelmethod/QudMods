using Kernelmethod.CrungleMode.Utilities;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// Zone X/Y/Z selector for the random biome.
    /// </summary>
    public class RandomXYZSelector : XYZSelector {
        public override int ChooseZ(string world, int parasangX, int parasangY)
        {
            if (Kernelmethod_CrungleMode_Random.Next(0, 1) == 0)
                return base.ChooseZ(world, parasangX, parasangY);

            return Kernelmethod_CrungleMode_Random.Next(1, 30);
        }
    }
}