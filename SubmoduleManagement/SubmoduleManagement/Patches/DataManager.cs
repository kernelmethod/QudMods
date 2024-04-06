using HarmonyLib;
using Kernelmethod.SubmoduleManagement;
using System.Collections.Generic;
using System.Linq;
using XRL;

namespace Kernelmethod.SubmoduleManagement.Patches {
    [HarmonyPatch(typeof(DataManager), nameof(DataManager.GetXMLFilesWithRoot))]
    public static class DataManagerGetXMLFilesWithRootPatch {
        public static void Postfix(ref List<DataFile> __result) {
            // Filter out files that have been excluded by the submodule
            // manager
            __result = __result
                .Where(file => SubmoduleManager.PathEnabled(file))
                .ToList();
        }
    }
}
