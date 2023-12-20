using System;
using System.Collections;
using System.Collections.Generic;
using XRL;
using XRL.CharacterBuilds.Qud;

namespace Kernelmethod.ChooseYourFighter {
    public class NoOpXmlHandler : IDictionary<string, Action<XmlDataHelper>> {
        private static readonly Dictionary<string, Action<XmlDataHelper>> EmptyDict = new Dictionary<string, Action<XmlDataHelper>>();

        public void HandleNodes(XmlDataHelper xml) {
            if (string.IsNullOrEmpty(xml.Name))
                return;
            xml.sanityChecks = false;
            xml.HandleNodes(this);
        }

        public void Add(string key, Action<XmlDataHelper> action) { return; }
        public void Clear() { return; }
        public bool Remove(string key) => true;
        IEnumerator<KeyValuePair<string, Action<XmlDataHelper>>> IEnumerable<KeyValuePair<string, Action<XmlDataHelper>>>.GetEnumerator() {
            return EmptyDict.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return EmptyDict.GetEnumerator();
        }

        public Action<XmlDataHelper> this[string key] {
            get {
                return HandleNodes;
            }
            set {
                return;
            }
        }

        public ICollection<string> Keys {
            get {
                return EmptyDict.Keys;
            }
        }
        public ICollection<Action<XmlDataHelper>> Values {
            get => EmptyDict.Values;
        }
        public int Count {
            get => 0;
        }
        public bool IsReadOnly {
            get => true;
        }

        public bool Contains(KeyValuePair<string, Action<XmlDataHelper>> kv) {
            return true;
        }
        public bool ContainsKey(string key) => true;
        public bool TryGetValue(string key, out Action<XmlDataHelper> value) {
            value = HandleNodes;
            return true;
        }

        public void CopyTo(KeyValuePair<string, Action<XmlDataHelper>>[] kv, int arrayIndex) { return; }
        public void Add(KeyValuePair<string, Action<XmlDataHelper>> kv) { return; }
        public bool Remove(KeyValuePair<string, Action<XmlDataHelper>> kv) => true;
    }

    [HasModSensitiveStaticCache]
    public static class PresetLoader {
        public static Dictionary<string, QudPregenModule.QudPregenData> Presets = new Dictionary<string, QudPregenModule.QudPregenData>();
        public static readonly NoOpXmlHandler NoOp = new NoOpXmlHandler();

        public static void DefaultNodeHandler(XmlDataHelper xml) {
            xml.sanityChecks = false;
            xml.HandleNodes(XmlNodeHandlers);
        }

        public static Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers => new Dictionary<string, Action<XmlDataHelper>>
        {
            { "embarkmodules", DefaultNodeHandler },
            {
                "module",
                delegate(XmlDataHelper xml)
                {
                    // Skip all modules except for QudPregenModule
                    if (xml.GetAttribute("Class") != "XRL.CharacterBuilds.Qud.QudPregenModule") {
                        xml.HandleNodes(NoOp);
                    }
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