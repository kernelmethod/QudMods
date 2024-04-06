using System;
using System.Collections.Generic;
using System.IO;
using XRL;
using XRL.Messages;
using XRL.World;
using XRL.UI;

namespace Kernelmethod.SubmoduleManagement {
    [HasModSensitiveStaticCache]
    public static class SubmoduleManager {
        [ModSensitiveStaticCache(true)]
        public static List<SubmoduleInfo> Submodules = new List<SubmoduleInfo>();

        private static string CurrentReadingSubmoduleFile = null;

        private static SubmoduleInfo CurrentReadingSubmodule = null;

        private static ModInfo CurrentReadingMod = null;

        [ModSensitiveCacheInit]
        public static void Initialize() {
            LogInfo("loading submodules");

            foreach (var file in DataManager.GetXMLFilesWithRoot("submodules"))
                AddSubmodule(file, file.Mod);

            LogInfo($"found {Submodules.Count} submodules");
        }

        public static void AddSubmodule(string Path, ModInfo modInfo) {
            LogInfo($"adding submodule found at {Path}");

            try {
                using XmlDataHelper stream = DataManager.GetXMLStream(Path, modInfo);
                CurrentReadingSubmoduleFile = Path;
                CurrentReadingMod = modInfo;
                HandleNodes(stream);
            }
            finally {
                CurrentReadingSubmoduleFile = null;
                CurrentReadingMod = null;
            }
        }

        public static bool PathEnabled(string Path) {
            foreach (var submod in Submodules) {
                if (submod.IsEnabled())
                    continue;

                if (Path.StartsWith(submod.Path)) {
                    LogInfo($"path disabled: {Path}");
                    return false;
                }
            }

            return true;
        }

        public static void HandleNodes(XmlDataHelper xml) {
            xml.HandleNodes(XmlNodeHandlers);
        }

        private static readonly Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers = new Dictionary<string, Action<XmlDataHelper>>
        {
            {
                "submodules",
                delegate (XmlDataHelper xml) {
                    xml.HandleNodes(XmlNodeHandlers);
                }
            },
            {
                "submodule",
                delegate (XmlDataHelper xml) {
                    if (CurrentReadingSubmodule != null)
                        throw new Exception($"already processing submodule at {CurrentReadingSubmodule.Path}; <submodule> tags cannot be nested inside one another");

                    try {
                        CurrentReadingSubmodule = new SubmoduleInfo();
                        var relPath = xml.GetAttribute("Path");
                        var parent = Directory.GetParent(CurrentReadingSubmoduleFile);

                        if (parent == null)
                            throw new Exception($"unable to retrieve parent for current submodule file being processed: {CurrentReadingSubmoduleFile}");

                        CurrentReadingSubmodule.Path = Path.Combine(parent.ToString(), relPath);
                        CurrentReadingSubmodule.Mod = CurrentReadingMod;

                        xml.HandleNodes(SubmoduleXmlNodeHandlers);
                        Submodules.Add(CurrentReadingSubmodule);
                    }
                    finally {
                        CurrentReadingSubmodule = null;
                    }
                }
            }
        };

        private static readonly Dictionary<string, Action<XmlDataHelper>> SubmoduleXmlNodeHandlers = new Dictionary<string, Action<XmlDataHelper>>
        {
            // Enable the submodule with an option
            {
                "optionenable",
                delegate (XmlDataHelper xml) {
                    var optionID = xml.GetAttribute("ID");
                    CurrentReadingSubmodule.IsEnabled = () => {
                        return Options.GetOption(optionID).EqualsNoCase("Yes");
                    };
                }
            }
        };

        private static void LogInfo(string message) {
            MetricsManager.LogInfo($"Kernelmethod_SubmoduleManagement::SubmoduleManager: {message}");
        }
    }
}
