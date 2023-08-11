using Kernelmethod.CrungleMode.Utilities;
using System;

namespace Kernelmethod.CrungleMode.ZoneSampling {
    /// <summary>
    /// Dynamic class to choose the X, Y, and Z coordinates of a random zone to sample (within a
    /// given parasang).
    /// </summary>
    public class XYZSelector {
        public XYZSelector() {}

        /// <summary>
        /// Choose the X, Y, and Z coordinates for the zone to sample random creatures from.
        /// In general only ChooseX, ChooseY, or ChooseZ need to be overriden; but you can override
        /// this method to ensure non-independent sampling of the X, Y, and Z coordinates.
        /// </summary>
        public virtual Tuple<int,int,int> ChooseXYZ(string world, int parasangX, int parasangY) {
            var coordX = ChooseX(world, parasangX, parasangY);
            var coordY = ChooseY(world, parasangX, parasangY);
            var coordZ = ChooseZ(world, parasangX, parasangY);
            return Tuple.Create(coordX, coordY, coordZ);
        }

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
