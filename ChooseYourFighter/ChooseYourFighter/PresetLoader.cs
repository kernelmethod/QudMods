using System;
using System.Collections.Generic;
using XRL;
using XRL.CharacterBuilds.Qud;

namespace Kernelmethod.ChooseYourFighter {
    [HasModSensitiveStaticCache]
    public static class PresetLoader {
        public static Dictionary<string, QudPregenModule.QudPregenData> Presets = new Dictionary<string, QudPregenModule.QudPregenData>();

        public static Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers => new Dictionary<string, Action<XmlDataHelper>>
        {
            {
                "embarkmodules",
                delegate(XmlDataHelper xml)
                {
                    xml.HandleNodes(XmlNodeHandlers);
                }
            },
            {
                "module",
                delegate(XmlDataHelper xml)
                {
                    // Skip all modules except for QudPregenModule
                    if (xml.GetAttribute("Class") != "XRL.CharacterBuilds.Qud.QudPregenModule")
                        return;
                    else {
                        var pregenModule = new QudPregenModule();
                        pregenModule.HandleNodes(xml);

                        foreach(var (key, value) in pregenModule.pregens) {
                            Presets[key] = value;
                        }
                    }
                }
            }
        };

        public static void HandleNodes(XmlDataHelper xml) {
            xml.HandleNodes(XmlNodeHandlers);
        }

        [ModSensitiveCacheInit]
        public static void Init() {
            LogInfo("Loading presets");

            foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("embarkmodules", IncludeMods: true)) {
                HandleNodes(item);
            }

            LogInfo($"Found {Presets.Count} presets");
        }

        private static void LogInfo(string message) {
            MetricsManager.LogInfo($"Kernelmethod_ChooseYourFighter::PresetLoader: {message}");
        }
    }
}