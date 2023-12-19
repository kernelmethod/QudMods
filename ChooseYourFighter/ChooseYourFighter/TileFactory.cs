using Qud.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using XRL;
using XRL.Language;
using XRL.UI;
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

            LogInfo("Finishing init...");
        }

        /// <summary>
        /// Create a menu for the player to change their appearance.
        /// </summary>
        public static PlayerModel ChooseTileMenu() {
            PlayerModel model = null;

            while (model == null) {
                var options = new List<string> {
                    "{{W|Choose tile from blueprint}}",
                    "Castes and callings"
                };

                if (HasExpansionModels()) {
                    options.Add("Expansions");
                }

                int num = Popup.ShowOptionList(
                    "Choose model",
                    options.ToArray(),
                    AllowEscape: true
                );

                if (num == 0)
                    model = GetModelFromBlueprint();
                else if (num == 1)
                    model = ChooseTileMenuWithCategory(ModelType.CasteOrCalling);
                else if (num == 2)
                    model = ChooseTileMenuWithCategory(ModelType.Expansion);
                else
                    break;
            }

            return model;
        }

        public static PlayerModel ChooseTileMenuWithCategory(ModelType category) {
            var models = new List<PlayerModel>(TileFactory.Models.Where(m => m.Category == category));
            models.Sort();

            models.Insert(0, new PlayerModel {
                Id="RETURN",
                Name="{{W|< go back}}"
            });

            var names = models.Select((PlayerModel m) => m.Name);
            var icons = models.Select((PlayerModel m) => m.Icon());

            int num = Popup.ShowOptionList(
                "Choose model",
                names.ToArray(),
                AllowEscape: true,
                Icons: icons.ToArray()
            );

            if (models[num].Id == "RETURN")
                return null;

            return models[num];
        }

        /// <summary>
        /// Change a player's appearance using a given PlayerModel instance as a
        /// blueprint.
        /// </summary>
        public static void ChangePlayerAppearance(PlayerModel model) {
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

            var message = "You changed your appearance to look like ";
            string name = null;

            if (model.Blueprint != null) {
                var gameObject = model.Blueprint.createOne();
                name = gameObject.GetDisplayName(
                    int.MaxValue, null, null, NoColor: false, Short: true,
                    AsIfKnown: false, WithIndefiniteArticle: true, BaseOnly: false
                );
            }
            else {
                name = (Grammar.IndefiniteArticleShouldBeAn(model.Name) ? "an " : "a ") + model.Name;
            }

            message += name + ".";
            JournalAPI.AddAccomplishment("You re-cast yourself in the image of " + name, muralWeight: MuralWeight.Nil);

            Popup.Show(message);
        }

        public static PlayerModel GetModelFromBlueprint() {
            var input = Popup.AskString("Enter blueprint:", "", 999, 0, null, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false);
            var blueprint = GameObjectFactory.Factory.GetBlueprintIfExists(input);

            if (blueprint == null) {
                Popup.ShowFail($"The blueprint {input} could not be found.");
                return null;
            }

            var gameObject = blueprint.createOne();
            if (gameObject.GetTile() == null) {
                Popup.ShowFail($"No tile could be found for the blueprint {input}");
                return null;
            }

            var model = new PlayerModel(blueprint);
            model.Id = "BLUEPRINT:" + input;
            model.HFlip = true;
            return model;
        }

        private static void LogInfo(string message) {
            MetricsManager.LogInfo($"Kernelmethod_ChooseYourFighter::TileFactory: {message}");
        }
    }
}
