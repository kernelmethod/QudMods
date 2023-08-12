using Kernelmethod.CrungleMode.Utilities;
using System;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// Zone X/Y/Z selector for the Spindle / Tomb of the Eaters biome.
    /// </summary>
    public class SpindleXYZSelector : XYZSelector {
        public static int SultanTombChanceIn100 = 10;

        public override Tuple<int, int, int> ChooseXYZ(string world, int parasangX, int parasangY) {
            int X, Y, Z;
            var chooseSultanTomb = (Kernelmethod_CrungleMode_Random.Next(1, 100) < SultanTombChanceIn100);

            if (chooseSultanTomb) {
                X = 1;
                Y = 0;
                Z = Kernelmethod_CrungleMode_Random.Next(0, 5);
                goto Exit;
            }

            X = ChooseX(world, parasangX, parasangY);
            Y = ChooseY(world, parasangX, parasangY);
            Z = Kernelmethod_CrungleMode_Random.Next(7, 11);

            Exit:
            return Tuple.Create(X, Y, Z);
        }
    }
}
