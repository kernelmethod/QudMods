using Qud.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRL;
using XRL.Language;
using XRL.Messages;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace Kernelmethod.ChooseYourFighter {
    [HasModSensitiveStaticCache]
    public static class TileFactory {
        [ModSensitiveStaticCache(false)]
        private static PlayerModel currentReadingModelData;

        [ModSensitiveStaticCache(false)]
        private static string currentReadingGroupId = null;

        [ModSensitiveStaticCache(false)]
        private static Dictionary<string, PlayerModel> _Models = null;

        /// <summary>
        /// Return a dictionary containing all of the possible player models, keyed to
        /// model IDs.
        /// </summary>
        public static Dictionary<string, PlayerModel> ModelDict {
            get {
                if (_Models == null)
                    Init();

                return _Models;
            }
        }

        /// <summary>
        /// Return an iterator over all of the usable player models.
        /// </summary>
        public static IEnumerable<PlayerModel> Models {
            get {
                foreach (var model in ModelDict.Values)
                    yield return model;
            }
        }

        /// <summary>
        /// Return an iterator over all of the player models belonging to a particular category.
        /// </summary>
        public static IEnumerable<PlayerModel> ModelsFromCategory(ModelType category) {
            foreach (var model in TileFactory.Models.Where(m => m.Category == category))
                yield return model;
        }

        /// <summary>
        /// Returns true if we have any character models from CYF expansions.
        /// </summary>
        public static bool HasExpansionModels() {
            return ModelDict.Values.Any(model => model.Category == ModelType.Expansion);
        }

        public static Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers => new Dictionary<string, Action<XmlDataHelper>>
        {
            {
                "KernelmethodChooseYourFighter",
                delegate(XmlDataHelper xml)
                {
                    xml.HandleNodes(XmlNodeHandlers);
                }
            },
            {
                "group",
                delegate(XmlDataHelper xml)
                {
                    if (currentReadingGroupId != null)
                        throw new Exception($"already processing the group {currentReadingGroupId}; you cannot nest tile groups inside one another");

                    // Add a player option corresponding to the new group
                    try {
                        currentReadingGroupId = xml.GetAttribute("ID");
                        var groupChoice = new PlayerModel();
                        groupChoice.Id = currentReadingGroupId;
                        groupChoice.Name = xml.GetAttribute("Name");
                        groupChoice.Category = ModelType.Expansion;
                        groupChoice.IsGroup = true;
                        _Models.Add(currentReadingGroupId, groupChoice);

                        xml.HandleNodes(XmlNodeHandlers);
                    }
                    finally {
                        currentReadingGroupId = null;
                    }
                }
            },
            {
                "model",
                delegate(XmlDataHelper xml)
                {
                    string id = xml.GetAttribute("ID");

                    try {
                        if (!_Models.TryGetValue(id, out currentReadingModelData)) {
                            currentReadingModelData = new PlayerModel();
                            _Models.Add(id, currentReadingModelData);
                        }

                        currentReadingModelData.Id = id;
                        currentReadingModelData.Name = xml.GetAttribute("Name");
                        currentReadingModelData.Group = currentReadingGroupId;
                        currentReadingModelData.Category = ModelType.Expansion;

                        xml.HandleNodes(XmlNodeHandlers);
                    }
                    finally {
                        currentReadingModelData = null;
                    }
                }
            },
            {
                "tile",
                delegate(XmlDataHelper xml)
                {
                    if (xml.HasAttribute("Path"))
                        currentReadingModelData.Tile = xml.GetAttribute("Path");
                    if (xml.HasAttribute("Foreground"))
                        currentReadingModelData.Foreground = xml.GetAttribute("Foreground");
                    if (xml.HasAttribute("Background"))
                        currentReadingModelData.Background = xml.GetAttribute("Background");
                    if (xml.HasAttribute("DetailColor"))
                        currentReadingModelData.DetailColor = xml.GetAttribute("DetailColor");
                    if (xml.HasAttribute("HFlip")) {
                        var HFlip = xml.GetAttribute("HFlip");
                        currentReadingModelData.HFlip = (HFlip == null) ? false : bool.Parse(HFlip);
                    }
                }
            }
        };

        public static void HandleNodes(XmlDataHelper xml) {
            xml.HandleNodes(XmlNodeHandlers);
        }

        [ModSensitiveCacheInit]
        public static void Init() {
            LogInfo("Initializing models dictionary");
            _Models = new Dictionary<string, PlayerModel>();

            // Populate dictionary with castes/callings
            foreach (var entry in SubtypeFactory.Subtypes) {
                var id = entry.Name;
                var model = new PlayerModel {
                    Id=id,
                    Name="{{M|" + entry.DisplayName + "}}",
                    Tile=entry.Tile,
                    DetailColor=entry.DetailColor,
                    Category=ModelType.CasteOrCalling,
                };
                _Models.Add(id, model);
            }

            LogInfo("Parsing model info from ChooseYourFighter.xml");

            foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("KernelmethodChooseYourFighter", IncludeMods: true)) {
                HandleNodes(item);
            }

            // Population dictionary with pregens
            foreach (var (key, value) in PresetLoader.Presets) {
                var id = value.Name;
                var model = new PlayerModel {
                    Id=id,
                    Name=value.Name,
                    Tile=value.Tile,
                    DetailColor=value.Detail,
                    Category=ModelType.Preset
                };
                _Models.Add(id, model);
            }

            LogInfo("Finishing init...");
        }

        /// <summary>
        /// Change an object's appearance using a given PlayerModel instance as a
        /// blueprint.
        /// </summary>
        public static void ChangeAppearance(GameObject Object, PlayerModel model, bool RequireDefault = true) {
            if (model == null)
                return;

            if (RequireDefault)
                Object.RequirePart<DefaultModel>();

            if (model.Tile != null)
                Object.Render.Tile = model.Tile;
            if (model.DetailColor != null)
                Object.Render.DetailColor = model.DetailColor;
            if (model.Foreground != null)
                Object.Render.SetForegroundColor(model.Foreground[0]);
            if (model.Background != null)
                Object.Render.SetBackgroundColor(model.Background[0]);

            LogInfo($"CheckFlip = {CheckFlip(Object)}");
            Object.Render.HFlip = CheckFlip(Object) ? model.HFlip : !model.HFlip;
        }

        public static bool CheckFlip(GameObject Object) {
            if (Object == null)
                return false;

            if (Object.IsPlayer() && Object.IsOriginalPlayerBody()) {
                return !Options.OptionNoPlayerTileFlipOriginalBody;
            }

            if (Object.IsPlayer() || Object.IsPlayerLed())
                return false;

            return true;
        }

        private static void LogInfo(string message) {
            MetricsManager.LogInfo($"Kernelmethod_ChooseYourFighter::TileFactory: {message}");
        }
    }
}
