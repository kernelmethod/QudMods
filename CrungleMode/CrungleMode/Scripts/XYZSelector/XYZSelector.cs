using Kernelmethod.CrungleMode.Utilities;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// Dynamic class to choose the X, Y, and Z coordinates of a random zone to sample (within a
    /// given parasang).
    /// </summary>
    public class XYZSelector {
        public XYZSelector() {}

        public virtual int ChooseX(string world, int parasangX, int parasangY) {
            return Kernelmethod_CrungleMode_Random.Next(0, 2);
        }

        public virtual int ChooseY(string world, int parasangX, int parasangY) {
            return Kernelmethod_CrungleMode_Random.Next(0, 2);
        }

        public virtual int ChooseZ(string world, int parasangX, int parasangY) {
            return 10;
        }
    }
}