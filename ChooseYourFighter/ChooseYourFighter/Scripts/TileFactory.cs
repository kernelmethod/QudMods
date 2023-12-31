using Qud.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using XRL;
using XRL.Language;
using XRL.Messages;
using XRL.World;
using XRL.World.Parts;

namespace Kernelmethod.ChooseYourFighter {
    [HasModSensitiveStaticCache]
    public static class TileFactory {
        [ModSensitiveStaticCache(false)]
        private static PlayerModel currentReadingModelData;

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
                "model",
                delegate(XmlDataHelper xml)
                {
                    string id = xml.GetAttribute("ID");

                    if (!_Models.TryGetValue(id, out currentReadingModelData)) {
                        currentReadingModelData = new PlayerModel();
                        currentReadingModelData.Category = ModelType.Expansion;
                        _Models.Add(id, currentReadingModelData);
                    }

                    currentReadingModelData.Id = id;
                    currentReadingModelData.Name = xml.GetAttribute("Name");

                    xml.HandleNodes(XmlNodeHandlers);
                    currentReadingModelData = null;
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
                    Name="{{M|" + value.Name + "}}",
                    Tile=value.Tile,
                    DetailColor=value.Detail,
                    Category=ModelType.Preset
                };
                _Models.Add(id, model);
            }

            LogInfo("Finishing init...");
        }

        /// <summary>
        /// Change a player's appearance using a given PlayerModel instance as a
        /// blueprint.
        /// </summary>
        public static void ChangePlayerAppearance(PlayerModel model, bool RequireDefault = true) {
            if (model == null)
                return;

            if (RequireDefault)
                The.Player.RequirePart<DefaultModel>();

            var part = The.Player.RequirePart<Render>();
            if (model.Tile != null)
                part.Tile = model.Tile;
            if (model.DetailColor != null)
                part.DetailColor = model.DetailColor;
            if (model.Foreground != null)
                part.SetForegroundColor(model.Foreground[0]);
            if (model.Background != null)
                part.SetBackgroundColor(model.Background[0]);

            if (model.HFlip)
                The.Player.RequirePart<Kernelmethod_ChooseYourFighter_FlipTile>();
            else
                The.Player.RemovePart("Kernelmethod_ChooseYourFighter_FlipTile");

            if (model.Name != null) {
                var message = "You changed your appearance to look like ";
                string name = null;


                name = (Grammar.IndefiniteArticleShouldBeAn(model.Name) ? "an " : "a ") + model.Name;

                message += name + ".";
                MessageQueue.AddPlayerMessage(message);
            }
        }

        private static void LogInfo(string message) {
            MetricsManager.LogInfo($"Kernelmethod_ChooseYourFighter::TileFactory: {message}");
        }
    }
}
