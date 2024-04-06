using System;
using XRL;

namespace Kernelmethod.SubmoduleManagement {
    public class SubmoduleInfo {
        /// <summary>
        /// Absolute path to the submodule that should be enabled or disabled.
        /// </summary>
        public string Path;

        /// <summary>
        /// Function that returns whether or not the submodule should be
        /// enabled.
        /// </summary>
        public Func<bool> IsEnabled = () => false;

        /// <summary>
        /// Mod that the file belongs to.
        /// </summary>
        public ModInfo Mod;
    }
}
